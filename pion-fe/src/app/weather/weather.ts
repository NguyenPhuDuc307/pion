import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from '../../environments/environment';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'app-weather',
  imports: [CommonModule],
  templateUrl: './weather.html',
  styleUrl: './weather.css'
})
export class WeatherComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];
  private readonly apiUrl = environment.apiUrl;

  constructor(private readonly http: HttpClient) { }

  ngOnInit(): void {
    this.getForecasts();
  }

  getForecasts() {
    this.http.get<WeatherForecast[]>(`${this.apiUrl}/weatherforecast`).subscribe({
      next: (result) => {
        this.forecasts = result;
      },
      error: (error) => {
        console.error(error);
      }
    });
  }
}
