<template>
  <div class="bg-dark text-white p-3 vh-100">
    <h1 class="text-white h5 me-3">
      <img src="images/mslogo.png" />
      <span>Video Indexer Console</span>
    </h1>

    <hr />

    <ul class="nav nav-pills flex-column mb-auto">
      <li class="nav-item">
        <router-link to="/" class="nav-link text-white" active-class="active" exact>
          <i class="bi bi-grid-1x2-fill"></i>
          <span class="ps-2">アップロードされたビデオ</span>
        </router-link>
      </li>
    </ul>
  </div>
</template>

<style scoped>
nav {
  width: 350px;
}

h1 img {
  height: 25px;
  position: relative;
  top: -2px;
  margin-right: 8px;
}
</style>
