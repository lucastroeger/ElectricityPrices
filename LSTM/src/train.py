import os
from datetime import datetime, timedelta
from data_fetcher import DataFetcher
from model import LSTMPredictor
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from sklearn.metrics import mean_squared_error, mean_absolute_error

def plot_predictions(y_true, y_pred, title="Predictions vs Actual"):
    plt.figure(figsize=(12, 6))
    plt.plot(y_true, label='Actual')
    plt.plot(y_pred, label='Predicted')
    plt.title(title)
    plt.xlabel('Time')
    plt.ylabel('Price')
    plt.legend()
    plt.savefig('predictions.png')
    plt.close()

def main():
    # Initialize data fetcher
    fetcher = DataFetcher()
    
    # Get data for the last 30 days
    end_date = datetime.now()
    start_date = end_date - timedelta(days=30)
    
    # Fetch and prepare data
    data = fetcher.prepare_training_data(
        start_date=start_date,
        end_date=end_date,
        bidding_zone="10YFI-1--------U",  # Finland
        latitude=60.1699,  # Helsinki
        longitude=24.9384
    )
    
    # Initialize model with PyTorch
    model = LSTMPredictor(
        sequence_length=24,
        n_features=data.shape[1],
        hidden_size=50,
        num_layers=2
    )
    
    # Scale the data
    scaled_data = model.scaler.fit_transform(data)
    
    # Prepare sequences
    X, y = model.prepare_sequences(scaled_data)
    
    # Split into train and test sets
    train_size = int(len(X) * 0.8)
    X_train, X_test = X[:train_size], X[train_size:]
    y_train, y_test = y[:train_size], y[train_size:]
    
    # Train the model
    model.train(
        X_train, 
        y_train, 
        epochs=100,
        batch_size=32,
        learning_rate=0.001
    )
    
    # Save the model
    os.makedirs("models", exist_ok=True)
    model.save_model("models/lstm_model.pt")
    
    # Evaluate the model
    predictions = model.predict(X_test)
    
    # Calculate metrics
    mse = mean_squared_error(y_test, predictions)
    mae = mean_absolute_error(y_test, predictions)
    rmse = np.sqrt(mse)
    
    print(f"Test MSE: {mse:.4f}")
    print(f"Test MAE: {mae:.4f}")
    print(f"Test RMSE: {rmse:.4f}")
    
    # Plot predictions
    plot_predictions(y_test, predictions)

if __name__ == "__main__":
    main() 