<template>
  <header>
    <nav>
      <ul class="nav-list">
        <li class="nav-item">
          <router-link to="/">🏠 Home</router-link>
        </li>
        <li class="nav-item">
          <router-link to="/create-portfolio">📁 Create Portfolio</router-link>
        </li>
        <li class="nav-item">
          <router-link to="/view-portfolio">📊 View Portfolio</router-link>
        </li>
      </ul>
    </nav>
  </header>

  <main>
    <router-view />
  </main>
</template>

<script>
export default {
  name: 'App'
}
</script>

<style>
body {
  background-color: #f9f9f9;
  margin: 0;
  padding: 0;
  font-family: Avenir, Helvetica, Arial, sans-serif;
}

#app {
  color: #333;
  line-height: 1.6;
}

header {
  background-color: #fff;
  border-bottom: 1px solid #eee;
  padding: 10px 0;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.nav-list {
  margin: 0;
  padding: 0;
  text-align: center;
  list-style: none;
}

.nav-item {
  display: inline-block;
  padding: 10px 20px;
  font-size: 18px;
  border-right: 1px solid #ddd;
}

.nav-item:last-child {
  border-right: none;
}

.nav-item a {
  color: #555;
  text-decoration: none;
  transition: color 0.3s;
}

.nav-item a:hover {
  color: #42b983;
  cursor: pointer;
}

.router-link-active {
  color: #42b983 !important;
  font-weight: bold;
}

main {
  margin: 30px auto;
  padding: 20px;
  max-width: 800px;
}
</style>