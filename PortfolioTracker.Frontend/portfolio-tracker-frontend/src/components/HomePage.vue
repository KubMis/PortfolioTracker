<template>
  <section>
    <header>
      <h1>{{ msg }}</h1>
    </header>
    <article>
      <p> Manage your investments efficiently and track your progress.</p>
    </article>
    <section class="info-section">
      <h2>About This App</h2>
      <p>This app helps you manage and track your investments like a true master. Add, view, and vibe with your portfolios all in one spot. 🚀</p>
    </section>
    <footer>
      <p>&copy; {{year}} Portfolio Tracker</p>
    </footer>
  </section>
</template>

<script>
export default {
  name: 'HelloWorld',
  props: {
    msg: String,
    year: Number
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h1, h2, p{
  text-align: center;
  margin: 20px 0;
}

.info-section {
  margin-top: 40px;
  background-color: #fff;
  padding: 20px;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.info-section h2 {
  margin-top: 0;
  color: #2c3e50;
}

.info-section p {
  color: #666;
}

footer {
  text-align: center;
  margin-top: 20px;
  color: #999;
}
</style>
