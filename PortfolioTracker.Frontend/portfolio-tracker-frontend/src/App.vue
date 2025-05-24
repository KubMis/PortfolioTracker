<template>
  <header>
    <nav>
      <ul class="nav-list">
        <li class="nav-item">🏠 Home</li>
        <li class="nav-item">📁 Create Portfolio</li>
        <li class="nav-item">📊 View Portfolio</li>
      </ul>
    </nav>
  </header>

  <main>
    <HomePage msg="Welcome to the Portfolio Tracker App." :year="getCurrentYear()"/>
  </main>
</template>

<script>
import HomePage from './components/HomePage.vue'

export default {
  name: 'App',
  components: {
    HomePage
  },
  methods: {
    getCurrentYear() {
      return new Date().getFullYear();
    }
  }
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
  color: #555;
  text-decoration: none;
  border-right: 1px solid #ddd;
  transition: color 0.3s;
}

.nav-item:last-child {
  border-right: none;
}

.nav-item:hover {
  color: #42b983;
  cursor: pointer;
}

main {
  margin: 30px auto;
  padding: 20px;
  max-width: 800px;
}

</style>
