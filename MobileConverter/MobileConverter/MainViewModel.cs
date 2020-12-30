using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Input;
using System.Text.Json;
using Xamarin.Forms;


namespace MobileConverter
{
	[DesignTimeVisible(true)]
	class MainViewModel : INotifyPropertyChanged
	{

		public MainViewModel()
		{
			text = "";
			activInditificator = false;
			currencysFirst = new ObservableCollection<CurrencyInfo>();
			selectedCurrencyFirst = null;
			selectedCurrencySecond = null;
			nominalFirst = 1;
			nominalSecond = 1;
			selectedDate = DateTime.Now;
			idSelectedFirst = -1;
			idSelectedSecond = -1;
		}

		private string text;
		private bool activInditificator;
		private ObservableCollection<CurrencyInfo> currencysFirst;
		private CurrencyInfo selectedCurrencyFirst;
		private CurrencyInfo selectedCurrencySecond;
		private double nominalFirst;
		private double nominalSecond;
		private DateTime selectedDate;
		private int idSelectedFirst;
		private int idSelectedSecond;

		public int IdSelectedFirst
		{
			get => idSelectedFirst;
			set
			{
				idSelectedFirst = value;
				OnPropertyChanged(nameof(IdSelectedFirst));
			}
		}

		public int IdSelectedSecond
		{
			get => idSelectedSecond;
			set
			{
				idSelectedSecond = value;
				OnPropertyChanged(nameof(IdSelectedSecond));
			}
		}

		public DateTime SelectedDate
		{
			get => selectedDate;
			set
			{
				selectedDate = value;
				OnPropertyChanged(nameof(SelectedDate));
			}
		}

		public double NominalFirst
		{
			get => nominalFirst;
			set
			{
				if (nominalFirst == value) return;
				if (SelectedCurrencySecond == SelectedCurrencyFirst)
				{
					value = 1;
				}
				nominalFirst = value;
				OnPropertyChanged(nameof(NominalFirst));
			}
		}

		public double NominalSecond
		{
			get => nominalSecond;
			set
			{
				if (nominalSecond == value) return;
				if (SelectedCurrencySecond == SelectedCurrencyFirst)
				{
					value = 1;
				}
				nominalSecond = value;
				OnPropertyChanged(nameof(NominalSecond));
			}
		}

		public string Text
		{
			get => text;
			set
			{
				text = value;
				OnPropertyChanged(nameof(Text));
			}
		}

		public bool ActivInditificator
		{
			get => activInditificator;
			set
			{
				activInditificator = value;
				OnPropertyChanged(nameof(ActivInditificator));
			}
		}

		public ObservableCollection<CurrencyInfo> CurrencysFirst
		{
			get => currencysFirst;
			set
			{
				currencysFirst = value;
				OnPropertyChanged(nameof(CurrencysFirst));
			}
		}

		public CurrencyInfo SelectedCurrencyFirst
		{
			get => selectedCurrencyFirst;
			set
			{
				selectedCurrencyFirst = value;
				OnPropertyChanged(nameof(SelectedCurrencyFirst));
				if (SelectedCurrencySecond != null && selectedCurrencyFirst != null) SetNominalFirstExe();
			}
		}

		public CurrencyInfo SelectedCurrencySecond
		{
			get => selectedCurrencySecond;
			set
			{
				selectedCurrencySecond = value;
				OnPropertyChanged(nameof(SelectedCurrencySecond));
				if (SelectedCurrencyFirst != null && selectedCurrencyFirst != null) SetNominalFirstExe();
			}
		}

		private ICommand convertCommand;
		private ICommand setNominalFirst;
		private ICommand setNominalSecond;

		public ICommand ConvertCommand => convertCommand ?? (convertCommand = new RelayCommand(ConvertCommandExe));
		public ICommand SetNominalFirst => setNominalFirst ?? (setNominalFirst = new RelayCommand(SetNominalFirstExe));
		public ICommand SetNominalSecond => setNominalSecond ?? (setNominalSecond = new RelayCommand(SetNominalSecondExe));


		private void SetNominalFirstExe()
		{
			NominalSecond = Math.Round(NominalFirst * (SelectedCurrencyFirst.Value / SelectedCurrencyFirst.Nominal) / SelectedCurrencySecond.Value, 2);
		}

		private void SetNominalSecondExe()
		{
			NominalFirst = Math.Round(NominalSecond * (SelectedCurrencySecond.Value / SelectedCurrencySecond.Nominal) / SelectedCurrencyFirst.Value, 2);
		}

		public async void ConvertCommandExe()
		{
			Text = "Hi";
			ActivInditificator = true;
			var httpClient = new HttpClient();
			HttpResponseMessage response = null;
			if (SelectedDate == DateTime.Now)
			{
				response = await httpClient.GetAsync("https://www.cbr-xml-daily.ru/daily_json.js");
			}
			else
			{
				response = await httpClient.GetAsync($"https://www.cbr-xml-daily.ru/archive/{SelectedDate.Year}/{SelectedDate.Month}/{SelectedDate.Day}/daily_json.js");
			}

			int i = 0;
			while (!response.IsSuccessStatusCode)
			{
				SelectedDate = SelectedDate.AddDays(-1);
				i++;
				if (i == 62) SelectedDate = DateTime.Now;
				response = await httpClient.GetAsync($"https://www.cbr-xml-daily.ru/archive/{SelectedDate.Year}/{SelectedDate.Month}/{SelectedDate.Day}/daily_json.js");
			}

			var content = await response.Content.ReadAsStreamAsync();
			var result = await JsonSerializer.DeserializeAsync<DailyRate>(content);
			var tmpf = IdSelectedFirst;
			var tmps = IdSelectedSecond;
			SelectedCurrencyFirst = null;
			SelectedCurrencySecond = null;
			CurrencysFirst.Clear();
			var RUB = new CurrencyInfo();
			RUB.Value = 1;
			RUB.CharCode = "RUB";
			RUB.Nominal = 1;
			CurrencysFirst.Add(RUB);
			CurrencysFirst.Add(result.Valute.AUD);
			CurrencysFirst.Add(result.Valute.AZN);
			CurrencysFirst.Add(result.Valute.GBP);
			CurrencysFirst.Add(result.Valute.USD);
			CurrencysFirst.Add(result.Valute.EUR);
			if (tmpf != -1) SelectedCurrencyFirst = CurrencysFirst[tmpf];
			if (tmps != -1) SelectedCurrencySecond = CurrencysFirst[tmps];
			try
			{
				SetNominalFirstExe();
			}
			catch (Exception e)
			{

			}
			ActivInditificator = false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged == null)
				return;
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	class CurrencyInfo
	{
		public string CharCode { get; set; }
		public int Nominal { get; set; }
		public double Value { get; set; }
	}

	class Currency
	{
		public Currency()
		{

		}

		private CurrencyInfo aut;
		private CurrencyInfo azn;
		private CurrencyInfo gbp;
		private CurrencyInfo usd;
		private CurrencyInfo eur;

		public CurrencyInfo USD
		{
			get => usd;
			set
			{
				usd = value;
				OnPropertyChanged(nameof(USD));
			}
		}
		public CurrencyInfo EUR
		{
			get => eur;
			set
			{
				eur = value;
				OnPropertyChanged(nameof(EUR));
			}
		}
		public CurrencyInfo AUD
		{
			get => aut;
			set
			{
				aut = value;
				OnPropertyChanged(nameof(AUD));
			}
		}
		public CurrencyInfo AZN
		{
			get => azn;
			set
			{
				azn = value;
				OnPropertyChanged(nameof(AZN));
			}
		}
		public CurrencyInfo GBP
		{
			get => gbp;
			set
			{
				gbp = value;
				OnPropertyChanged(nameof(GBP));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged == null)
				return;
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	class DailyRate
	{
		public DateTime Date { get; set; }
		public DateTime PreviousDate { get; set; }
		public string PreviousURL { get; set; }
		public Currency Valute { get; set; }

	}
}
