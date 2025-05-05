import numpy as np
import torch
import torch.nn as nn
from sklearn.preprocessing import MinMaxScaler

class LSTMModel(nn.Module):
    def __init__(self, input_size, hidden_size, num_layers, output_size, dropout=0.2):
        super(LSTMModel, self).__init__()
        self.hidden_size = hidden_size
        self.num_layers = num_layers
        
        self.lstm = nn.LSTM(input_size, hidden_size, num_layers, batch_first=True, dropout=dropout)
        self.fc1 = nn.Linear(hidden_size, 25)
        self.fc2 = nn.Linear(25, output_size)
        self.dropout = nn.Dropout(dropout)
        
    def forward(self, x):
        # Initialize hidden state with zeros
        h0 = torch.zeros(self.num_layers, x.size(0), self.hidden_size).to(x.device)
        c0 = torch.zeros(self.num_layers, x.size(0), self.hidden_size).to(x.device)
        
        # Forward propagate LSTM
        out, _ = self.lstm(x, (h0, c0))
        
        # Decode the hidden state of the last time step
        out = self.dropout(out[:, -1, :])
        out = self.fc1(out)
        out = self.dropout(out)
        out = self.fc2(out)
        return out

class LSTMPredictor:
    def __init__(self, sequence_length=24, n_features=4, hidden_size=50, num_layers=2):
        self.sequence_length = sequence_length
        self.n_features = n_features
        self.hidden_size = hidden_size
        self.num_layers = num_layers
        
        # Initialize model and scaler
        self.model = LSTMModel(n_features, hidden_size, num_layers, 1)
        self.scaler = MinMaxScaler()
        
        # Set device
        self.device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
        self.model.to(self.device)
        
    def prepare_sequences(self, data):
        """Prepare sequences for LSTM input"""
        X, y = [], []
        for i in range(len(data) - self.sequence_length):
            X.append(data[i:(i + self.sequence_length)])
            y.append(data[i + self.sequence_length, 0])  # Assuming price is the first column
        return np.array(X), np.array(y)
    
    def train(self, X_train, y_train, epochs=50, batch_size=32, learning_rate=0.001):
        """Train the LSTM model"""
        # Convert data to PyTorch tensors
        X_train = torch.FloatTensor(X_train).to(self.device)
        y_train = torch.FloatTensor(y_train).to(self.device)
        
        # Define loss and optimizer
        criterion = nn.MSELoss()
        optimizer = torch.optim.Adam(self.model.parameters(), lr=learning_rate)
        
        # Training loop
        self.model.train()
        for epoch in range(epochs):
            for i in range(0, len(X_train), batch_size):
                batch_X = X_train[i:i+batch_size]
                batch_y = y_train[i:i+batch_size]
                
                # Forward pass
                outputs = self.model(batch_X)
                loss = criterion(outputs.squeeze(), batch_y)
                
                # Backward pass and optimize
                optimizer.zero_grad()
                loss.backward()
                optimizer.step()
            
            if (epoch + 1) % 10 == 0:
                print(f'Epoch [{epoch+1}/{epochs}], Loss: {loss.item():.4f}')
    
    def predict(self, X):
        """Make predictions using the trained model"""
        self.model.eval()
        with torch.no_grad():
            X = torch.FloatTensor(X).to(self.device)
            predictions = self.model(X)
        return predictions.cpu().numpy()
    
    def save_model(self, path):
        """Save the trained model"""
        torch.save({
            'model_state_dict': self.model.state_dict(),
            'scaler': self.scaler
        }, path)
    
    def load_model(self, path):
        """Load a trained model"""
        checkpoint = torch.load(path)
        self.model.load_state_dict(checkpoint['model_state_dict'])
        self.scaler = checkpoint['scaler'] 