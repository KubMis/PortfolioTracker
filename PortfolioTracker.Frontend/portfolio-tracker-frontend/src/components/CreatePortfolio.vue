<template>
  <div class="create-portfolio">
    <h1>Create new portfolio</h1>

    <div class="form-group">
      <label for="portfolioName">Portfolio name:</label>
      <input type="text" id="portfolioName" v-model="portfolioName" class="form-control"/>
    </div>

    <div class="ticker-selection">
      <h2>Select tickers</h2>

      <div class="search-container">
        <input type="text" placeholder="Search for ticker" v-model="searchQuery" class="search-input"/>
      </div>

      <div v-if="loading" class="loading">Loading tickers...</div>
      <div v-else-if="error" class="error-message">{{ error }}</div>
      <div v-else class="ticker-list">
        <div v-for="ticker in filteredTickers" :key="ticker.tickerSymbol" class="ticker-item">
          <label class="ticker-label">
            <input type="checkbox" v-model="ticker.selected"/>
            <span class="ticker-symbol">{{ ticker.tickerSymbol }}</span>
            <span class="ticker-name">{{ ticker.companyName }}</span>
            <span class="share-number">
              <input type="number" v-model.number="ticker.numberOfShares" min="0" class="share-input styled-input"
                     placeholder="Number of shares"/>
            </span>
            <span class="average-share-price">
              <input type="number" v-model.number="ticker.averageSharePrice" min="0" class="share-input styled-input"
                     placeholder="Average share price"/>
            </span>
          </label>
        </div>
      </div>

      <div class="selected-info" v-if="selectedTickers.length > 0">
        Selected: {{ selectedTickers.length }} tickers
      </div>
    </div>

    <div class="actions">
      <button @click="savePortfolio" :disabled="!isValid" class="save-button">
        Create portfolio
      </button>
    </div>
  </div>
</template>

<script>
export default {
  name: 'CreatePortfolio',
  data() {
    return {
      portfolioName: '',
      tickers: [],
      loading: true,
      error: null,
      searchQuery: ''
    }
  },
  computed: {
    filteredTickers() {
      const query = this.searchQuery.toLowerCase();
      return this.tickers.filter(t =>
          !this.searchQuery ||
          t.tickerSymbol.toLowerCase().includes(query) ||
          t.companyName.toLowerCase().includes(query)
      );
    },
    selectedTickers() {
      return this.tickers
          .filter(t => t.selected)
          .map(t => ({
            tickerSymbol: t.tickerSymbol,
            numberOfShares: t.numberOfShares || 0,
            averageSharePrice: t.averageSharePrice || 0
          }));
    },
    isValid() {
      return (
          this.portfolioName.trim() !== '' &&
          this.selectedTickers.length > 0 &&
          this.selectedTickers.every(
              t => t.numberOfShares > 0 && t.averageSharePrice > 0 
          )
      );
    }
  },
  mounted() {
    this.fetchTickers();
  },
  methods: {
    async fetchTickers() {
      this.loading = true;
      try {
        const response = await fetch('http://localhost:5013/api/Ticker/GetAllTickers');
        if (!response.ok) throw new Error('Failed to fetch ticker list');
        const data = await response.json();
        this.tickers = data.map(t => ({
          ...t,
          selected: false,
          numberOfShares: null,
          averageSharePrice: null
        }));
      } catch (err) {
        this.error = 'Error during fetching: ' + err.message;
        console.error(err);
      } finally {
        this.loading = false;
      }
    },
    async savePortfolio() {
      if (!this.isValid) return;

      try {
        const response = await fetch('http://localhost:5013/api/Portfolio/CreatePortfolio', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            portfolioName: this.portfolioName,
            tickerList: this.selectedTickers
          })
        });

        if (!response.ok) throw new Error('Failed to create portfolio');

        alert('Portfolio created successfully. ✌️');
        this.portfolioName = '';
        this.tickers.forEach(t => {
          t.selected = false;
          t.numberOfShares = null;
          t.averageSharePrice = null;
        });
      } catch (err) {
        alert('Error occurred: ' + err.message);
        console.error(err);
      }
    }
  }
}
</script>

<style scoped>
.create-portfolio {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
}

.form-group {
  margin-bottom: 20px;
}

.form-control {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.ticker-selection {
  margin-top: 20px;
}

.search-input {
  width: 100%;
  padding: 8px;
  margin-bottom: 15px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.ticker-list {
  max-height: 400px;
  overflow-y: auto;
  border: 1px solid #eee;
  border-radius: 4px;
}

.ticker-item {
  padding: 8px 12px;
  border-bottom: 1px solid #eee;
  display: flex;
}

.ticker-item:last-child {
  border-bottom: none;
}

.ticker-label {
  display: flex;
  align-items: center;
  width: 100%;
  cursor: pointer;
}

.ticker-symbol {
  font-weight: bold;
  margin-left: 10px;
  width: 80px;
}

.ticker-name {
  margin-left: 15px;
  color: #555;
}

.selected-info {
  margin-top: 15px;
  font-weight: bold;
}

.actions {
  margin-top: 25px;
}

.save-button {
  padding: 10px 20px;
  background-color: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  
}

.save-button:disabled {
  background-color: #cccccc;
  cursor: not-allowed;
}

.loading, .error-message {
  padding: 20px;
  text-align: center;
}

.error-message {
  color: #f44336;
}
</style>
