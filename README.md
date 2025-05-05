# Electricity Prices App

A web application that visualizes electricity prices and weather data, with an LSTM model for price predictions.

## Features

- Real-time electricity price visualization for different bidding zones
- Weather data integration (temperature, wind speed, solar radiation)
- LSTM-based price prediction model
- REST API for price predictions

## Architecture

The application consists of two main components:

1. **Web Application (.NET)**
   - Frontend: ASP.NET Core MVC with Chart.js for visualizations
   - Backend: Services for fetching data from ENTSO-E and OpenMeteo APIs
   - Bidding zones supported:
     - Germany-Luxembourg (10Y1001A1001A82H)
     - Austria (10YAT-APG------L)

2. **LSTM Prediction Model (Python)**
   - FastAPI service for predictions
   - LSTM model using PyTorch
   - Data fetcher for ENTSO-E and OpenMeteo APIs

## Setup

### Prerequisites

- .NET 9.0
- Python 3.11
- ENTSO-E API token
- OpenMeteo API (no token required)

### Web Application Setup

1. Clone the repository
2. Add your ENTSO-E API token to `appsettings.Development.json`:
   ```json
   {
     "ApiKeys": {
       "EntsoE": "your_token_here"
     }
   }
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

### LSTM Model Setup

1. Navigate to the LSTM directory:
   ```bash
   cd LSTM
   ```

2. Create and activate a virtual environment:
   ```bash
   python -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   ```

3. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```

4. Start the FastAPI service:
   ```bash
   cd src
   python api.py
   ```

## API Endpoints

### Prediction API

- **URL**: `http://localhost:8000/predict`
- **Method**: POST
- **Request Body**:
  ```json
  {
    "bidding_zone": "DE",
    "latitude": 52.52,
    "longitude": 13.405,
    "hours_ahead": 24
  }
  ```
- **Response**:
  ```json
  {
    "predicted_price": 32.05,
    "timestamp": "2025-05-05T13:11:01.769410",
    "model_info": {
      "device": "cpu",
      "sequence_length": 24,
      "n_features": 4
    }
  }
  ```

## Project Structure

```
ElectricityPricesApp/
├── Controllers/         # .NET MVC controllers
├── Models/             # .NET data models
├── Services/           # .NET services for API integration
├── Views/              # .NET MVC views
├── LSTM/               # Python LSTM model
│   ├── src/
│   │   ├── api.py      # FastAPI service
│   │   ├── model.py    # LSTM model implementation
│   │   ├── data_fetcher.py  # Data fetching utilities
│   │   └── requirements.txt # Python dependencies
│   └── venv/           # Python virtual environment
└── appsettings.json    # Configuration file
```

## Current Status

- ✅ Web application with price and weather visualization
- ✅ LSTM model implementation
- ✅ FastAPI service for predictions
- ⚠️ ENTSO-E API integration (using dummy data)
- ⚠️ Model training (not implemented yet)

## Next Steps

1. Implement proper XML parsing for ENTSO-E API responses
2. Train the LSTM model with historical data
3. Add more bidding zones
4. Improve prediction accuracy
5. Add model evaluation metrics

## License

[Your License Here]
