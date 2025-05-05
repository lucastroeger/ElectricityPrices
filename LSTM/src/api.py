from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from datetime import datetime, timedelta
import numpy as np
from model import LSTMPredictor
from data_fetcher import DataFetcher

app = FastAPI()

class PredictionRequest(BaseModel):
    bidding_zone: str
    latitude: float
    longitude: float
    hours_ahead: int = 24

@app.post("/predict")
async def predict_prices(request: PredictionRequest):
    try:
        # Initialize components
        fetcher = DataFetcher()
        model = LSTMPredictor()
        
        # Get historical data for the last 24 hours
        end_date = datetime.now()
        start_date = end_date - timedelta(hours=24)
        
        # Fetch data
        data = fetcher.prepare_training_data(
            start_date=start_date,
            end_date=end_date,
            bidding_zone=request.bidding_zone,
            lat=request.latitude,
            lon=request.longitude
        )
        
        # Fit and transform the data
        model.scaler.fit(data)
        scaled_data = model.scaler.transform(data)
        
        # Prepare the sequence for prediction
        X = scaled_data[-model.sequence_length:].reshape(1, model.sequence_length, model.n_features)
        
        # Make prediction
        prediction = model.predict(X)
        
        # Inverse transform the prediction
        prediction = model.scaler.inverse_transform(
            np.concatenate([prediction, np.zeros((1, model.n_features-1))], axis=1)
        )[:, 0]
        
        return {
            "predicted_price": float(prediction[0]),
            "timestamp": datetime.now().isoformat(),
            "model_info": {
                "device": str(model.device),
                "sequence_length": model.sequence_length,
                "n_features": model.n_features
            }
        }
        
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail={
                "error": str(e),
                "type": type(e).__name__
            }
        )

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000) 