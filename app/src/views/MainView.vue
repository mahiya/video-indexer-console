<template>
  <div class="row p-0 m-0">

    <!-- 左エリア -->
    <div class="col py-6 px-3 vh-100" style="overflow-y: scroll;">
      <h2 class="h4 mb-3" style="margin-top: 15px">アップロードされたファイル</h2>

      <!-- ボタンエリア -->
      <div class="text-end">
        <button class="btn btn-primary" @click="listVideos()">最新情報に更新</button>
        <button class="btn btn-primary ms-2" @click="showUploadModal = true">アップロード</button>
        <button class="btn btn-danger ms-2" v-bind:disabled="!selectedVideo" @click="showDeleteModal = true">削除</button>
      </div>

      <!-- ビデオ一覧のテーブル -->
      <table class="table table-hover" v-if="!loading && videos.length > 0">
        <thead>
          <tr>
            <th>ファイル名</th>
            <th>状態</th>
            <th>時間</th>
            <th>アップロード日時</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="video in videos"
            :key="video.id"
            style="cursor: pointer"
            @click="onVideoSelected(video)"
            v-bind:class="{ 'table-active': selectedVideo && selectedVideo.id == video.id }"
          >
            <td>
              <img class="me-1 fileIcon" src="images/video.svg" />
              <span>{{video.name}}</span>
            </td>
            <td>
              <span>{{videoStateStr[video.state]}}</span>
              <span v-if="video.state == 'Processing'">({{video.processingProgress}})</span>
            </td>
            <td>{{formatDurationSeconds(video.durationInSeconds)}}</td>
            <td>{{video.created}}</td>
            <td><a href="#" @click="getArtifact(video)" v-if="video.state == 'Processed'">分析情報</a></td>
          </tr>
        </tbody>
      </table>

      <div v-if="!loading && videos.length == 0">ファイルがアップロードされていません</div>

      <!-- Loading アイコン -->
      <div class="text-center w-100" v-if="loading">
        <div>
          <div class="spinner spinner-border text-secondary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>
      </div>

      <!-- ビデオアップロードのモーダル -->
      <Teleport to="body">
        <Modal :show="showUploadModal" @close="showUploadModal = false">
          <template #headerTitle>ビデオのアップロード</template>
          <template #body>
            <div>
              <input type="file" @change="onUploadFileSelected" class="form-control" />
            </div>
            <p class="text-danger" v-if="uploadErrorMessage">{{uploadErrorMessage}}</p>
            <div class="progress" v-if="uploading">
              <div
                class="progress-bar progress-bar-striped progress-bar-animated"
                role="progressbar"
                v-bind:style="{ width: uploadProgress + '%' }"
              ></div>
            </div>
          </template>
          <template #footer>
            <button type="button" class="btn btn-secondary" @click="showUploadModal = false">閉じる</button>
          </template>
        </Modal>
      </Teleport>
      
      <!-- ビデオ削除のモーダル -->
      <Teleport to="body">
        <Modal :show="showDeleteModal" @close="showDeleteModal = false">
          <template #headerTitle>ビデオの削除</template>
          <template #body>選択したビデオ "{{selectedVideo.name}}" を削除しますか？</template>
          <template #footer>
            <button type="button" class="btn btn-secondary me-2" @click="showDeleteModal = false">閉じる</button>
            <button type="button" class="btn btn-danger" @click="deleteVideo(selectedVideo)">削除</button>
          </template>
        </Modal>
      </Teleport>
    </div>

    <!-- 右エリア -->
    <div class="col vh-100" style="overflow-y: scroll;">
      <!-- Loading アイコン -->
      <div class="text-center w-100" v-if="videoLoading">
        <div>
          <div class="spinner spinner-border text-secondary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>
      </div>

      <!-- ビデオウィジット -->
      <iframe
        class="w-100 mb-3"
        style="margin-top: 15px; height: 250px"
        v-if="videoWidgetsUrl != null"
        v-bind:src="videoWidgetsUrl"
        frameborder="0"
        allowfullscreen
      ></iframe>

      <!-- インサイトウィジット -->
      <iframe
        class="w-100"
        style="height: 600px"
        v-if="insightsWidgetsUrl != null"
        v-bind:src="insightsWidgetsUrl"
        frameborder="0"
        allowfullscreen
      ></iframe>
    </div>
  </div>
</template>


<script>
import axios from "axios";
import Modal from "../components/ModalView.vue";

export default {
  name: "MainView",
  components: {
    Modal,
  },
  data() {
    return {
      loading: false,
      videos: [],
      videoStateStr: {
        Uploaded: "アップロード済み",
        Processing: "分析中",
        Processed: "分析完了",
        Failed: "分析失敗",
        Quarantined: "検疫済み",
      },
      showUploadModal: false,
      showDeleteModal: false,
      uploading: false,
      uploadProgress: 0,
      uploadErrorMessage: null,
      selectedVideo: null,
      videoLoading: false,
      videoWidgetsUrl: null,
      insightsWidgetsUrl: null
    };
  },
  watch: {
    selectedVideo: async function() {
      if (!this.selectedVideo || this.selectedVideo.state != "Processed") return;
      this.videoWidgetsUrl = null;
      this.insightsWidgetsUrl = null;
      this.videoLoading = true;
      const url = `${this.webApiEndpoint}/api/videos/${this.selectedVideo.id}/widgets`;
      const resp = await axios.get(url);

      this.videoWidgetsUrl = resp.data.videoWidgetsUrl;
      this.insightsWidgetsUrl = resp.data.insightsWidgetsUrl;
      this.videoLoading = false;
    },
  },
  async mounted() {
    await this.listVideos();
  },
  methods: {
    // ビデオ一覧を取得する
    listVideos: async function () {
      this.loading = true;
      const url = `${this.webApiEndpoint}/api/videos`;
      const resp = await axios.get(url);
      this.loading = false;
      this.videos = resp.data.map((d) => {
        d.created = this.formatDate(d.created);
        return d;
      });

      // ビデオがアップロードされていない場合は、アップロード用のモーダルを表示する
      if (this.videos.length == 0) {
        this.showUploadModal = true;
      } else {
        // ビデオがアップロードされている場合は、一番上のビデオを選択された状態にする
        this.selectedVideo = this.videos[0];
      }
    },
    // アップロードファイルが選択された時の処理
    onUploadFileSelected: async function (e) {
      var file = e.target.files[0];
      if (!file) return;

      this.uploading = 0;
      this.uploadProgress = 0;
      this.uploadErrorMessage = null;

      // Azure Blob へのアップロード先URL(SAS付き)を取得する
      const url = `${this.webApiEndpoint}/api/videos/uploadurl?name=${file.name}`;
      const resp = await axios.get(url);
      const uploadUrl = resp.data;

      const reader = new FileReader();
      const onUploadProgress = (e) => {
        this.uploadProgress = Math.round(e.progress * 100);
        if (this.uploadProgress == 100) {
          setTimeout(() => { this.showUploadModal = false; }, 250); // モーダルを非表示にするのを多少遅らせる
          setTimeout(() => { this.uploading = false; }, 500); // プログレスバーを非表示にするのを多少遅らせる
          this.videos.push({
            name: file.name,
            state: "Uploaded",
            created: this.formatDate(new Date()),
          });
        }
      };
      reader.onload = async (e) => {
        const headers = {
          "x-ms-blob-type": "BlockBlob",
          "Content-Type": "audio/mp4",
        };
        this.uploading = true;
        await axios.put(uploadUrl, e.target.result, {
          headers,
          onUploadProgress,
        });
      };
      reader.readAsArrayBuffer(file);
    },
    // ビデオ一覧で任意のビデオがクリックされた時の処理
    onVideoSelected: async function (video) {
      this.selectedVideo = video;
    },
    // ビデオ分析情報を取得して画面に表示する処理
    getArtifact: async function(video) {
      const url = `${this.webApiEndpoint}/api/videos/${video.id}/artifact`;
      const resp = await axios.get(url);
      window.open(resp.data.artifactUrl, '_blank');
    },
    // 指定したビデオを削除する処理
    deleteVideo: async function(video) {
      const url = `${this.webApiEndpoint}/api/videos/${video.id}`;
      await axios.delete(url);
      this.listVideos(); // ビデオ一覧を再取得する
    },
    // 秒数(int)からビデオの再生時間テキスト(string)へ変換する処理
    formatDurationSeconds: function (seconds) {
      if (!seconds) return "";
      const pad = (number) => (number < 10 ? "0" + number : number);
      const hours = Math.floor(seconds / 3600);
      const minutes = Math.floor((seconds - hours * 3600) / 60);
      const remainingSeconds = seconds - hours * 3600 - minutes * 60;
      return pad(hours) + ":" + pad(minutes) + ":" + pad(remainingSeconds);
    },
    // アップロード日時を"yyyy/MM/dd HH:mm:ss"のフォーマットに変換する処理
    formatDate: function (date) {
      const d = new Date(date);
      const twoDig = (val) => ("0" + val).slice(-2);
      return `${d.getFullYear()}/${twoDig(d.getMonth() + 1)}/${twoDig(
        d.getDate()
      )} ${twoDig(d.getHours())}:${twoDig(d.getMinutes())}:${twoDig(
        d.getSeconds()
      )}`;
    }
  },
};
</script>

<style scoped>
img.fileIcon {
  height: 25px;
  position: relative;
  top: -1.5px;
}

.spinner {
  position: relative;
  top: 3.5rem;
  width: 5rem;
  height: 5rem;
}
</style>