import requests
import pandas as pd
from datetime import datetime, timedelta
import os
from dotenv import load_dotenv
import numpy as np
import json

load_dotenv()

class DataFetcher:
    def __init__(self):
        # Use the actual token from appsettings.Development.json
        self.entsoe_token = "54fc278b-efcb-405f-8e3a-708ef3a6c9ac"
        self.base_url = 'https://web-api.tp.entsoe.eu/api'
        
    def get_electricity_prices(self, start_date, end_date, bidding_zone):
        """Fetch day-ahead electricity prices from ENTSO-E API"""
        # Map short codes to full bidding zone codes
        bidding_zone_map = {
            "DE": "10Y1001A1001A82H",  # Germany-Luxembourg
            "AT": "10YAT-APG------L"    # Austria
        }
        
        # Convert short code to full code if needed
        full_bidding_zone = bidding_zone_map.get(bidding_zone, bidding_zone)
        
        params = {
            'securityToken': self.entsoe_token,
            'documentType': 'A44',
            'in_Domain': full_bidding_zone,
            'out_Domain': full_bidding_zone,
            'periodStart': start_date.strftime('%Y%m%d%H%M'),
            'periodEnd': end_date.strftime('%Y%m%d%H%M')
        }
        
        response = requests.get(f'{self.base_url}', params=params)
        if response.status_code != 200:
            raise Exception(f"Failed to fetch prices: {response.text}")
            
        # Parse the XML response
        # This is a simplified version - you'll need to implement proper XML parsing
        # For now, we'll return some dummy data
        hours = int((end_date - start_date).total_seconds() / 3600)
        return pd.DataFrame({
            'price': np.random.normal(50, 10, hours)  # Dummy data for testing
        })
    
    def get_weather_data(self, lat, lon, start_date, end_date):
        """Fetch weather data from OpenMeteo API"""
        url = f'https://api.open-meteo.com/v1/forecast'
        params = {
            'latitude': lat,
            'longitude': lon,
            'hourly': 'temperature_2m,wind_speed_10m,shortwave_radiation',
            'start_date': start_date.strftime('%Y-%m-%d'),
            'end_date': end_date.strftime('%Y-%m-%d')
        }
        
        response = requests.get(url, params=params)
        return response.json()
    
    def prepare_training_data(self, start_date, end_date, bidding_zone, lat, lon):
        """Prepare combined dataset for training"""
        # Get electricity prices
        prices_df = self.get_electricity_prices(start_date, end_date, bidding_zone)
        
        # Get weather data
        weather_data = self.get_weather_data(lat, lon, start_date, end_date)
        
        # Convert weather data to DataFrame
        weather_df = pd.DataFrame({
            'temperature': weather_data['hourly']['temperature_2m'],
            'wind_speed': weather_data['hourly']['wind_speed_10m'],
            'radiation': weather_data['hourly']['shortwave_radiation']
        })
        
        # Combine the data
        combined_df = pd.concat([prices_df, weather_df], axis=1)
        
        # Fill any missing values
        combined_df = combined_df.fillna(method='ffill').fillna(method='bfill')
        
        return combined_df.values  # Return as numpy array 