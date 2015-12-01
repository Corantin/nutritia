using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nutritia.UI.Ressources.Localisation;
using Infralution.Localization.Wpf;

namespace Nutritia.UI.Views
{
	/// <summary>
	/// Interaction logic for FenetreCalculatriceNutritionelle.xaml
	/// </summary>
	public partial class FenetreCalculatriceNutritionelle : UserControl
	{

		private List<Plat> PlateauPlat { get; set; }
		private List<Aliment> PlateauAliment { get; set; }
		// Cet Aliment sert de totaux de tous les Aliments/Ingrédients du plateau
		private Dictionary<string, double> ValeurNutritive { get; set; }
		private List<Plat> LstPlat { get; set; }
		private List<Aliment> LstAliment { get; set; }

		private List<Plat> BoiteRechPlat { get; set; }
		private List<Aliment> BoiteRechAliment { get; set; }

		private List<int?> lstIdPresent { get; set; }
		private SousEcran Plateau { get; set; }
		public SousEcran BoiteResultat { get; set; }
		private SousEcran2 TabValeurNutritionelle { get; set; }

		public List<int> LstIdPlatADerouler { get; set; }

		public FenetreCalculatriceNutritionelle()
		{
			InitializeComponent();
			Initialiser();
			
			// Header de la fenetre
			App.Current.MainWindow.Title = "Nutritia - Calculatrice nutritionnelle";

			ConfigurerCalculatrice();
		}

		public FenetreCalculatriceNutritionelle(Plat platEnvoye)
		{
            CultureManager.UICultureChanged += CultureManager_UICultureChanged;
			InitializeComponent();
			Initialiser();

			// On déssine le plateau s'il a un plat passé en paramètre (apellé ailleur que dans les menus principaux)
			PlateauPlat.Add(platEnvoye);

			ConfigurerCalculatrice();
		}

		public FenetreCalculatriceNutritionelle(List<Plat> lstPlatEnvoye)
		{
			InitializeComponent();
			Initialiser();

			// On déssine le plateau s'il y a des plat passés en paramètre (apellé ailleur que dans les menus principaux)
			PlateauPlat.AddRange(lstPlatEnvoye);

			ConfigurerCalculatrice();
		}

		private void Initialiser()
		{
			Plateau = new SousEcran();
			presenteurContenu2.Content = Plateau;
			TabValeurNutritionelle = new SousEcran2();
			presenteurContenu3.Content = TabValeurNutritionelle;
			presenteurContenu4.Content = BoiteResultat;


			LstPlat = new List<Plat>();
			LstAliment = new List<Aliment>();
			BoiteRechPlat = new List<Plat>();
			BoiteRechAliment = new List<Aliment>();

			PlateauPlat = new List<Plat>();
			PlateauAliment = new List<Aliment>();
			ValeurNutritive = new Dictionary<string, double>();

			LstIdPlatADerouler = new List<int>();
		}

		private void ConfigurerCalculatrice()
		{ 
			// On génere l'écran des valeurs nutritives
			CalculerValeurNutritionelle();

			Mouse.OverrideCursor = Cursors.Wait;

			LstPlat.AddRange(ServiceFactory.Instance.GetService<IPlatService>().RetrieveAll());
			LstAliment.AddRange(ServiceFactory.Instance.GetService<IAlimentService>().RetrieveAll());

			BoiteRechPlat.AddRange(LstPlat);
			BoiteRechAliment.AddRange(ServiceFactory.Instance.GetService<IAlimentService>().RetrieveAll());

			// Dispatch des aliment trié par nom dans les différentes parties de l'accordéon
			LstPlat = LstPlat.OrderBy(plat => plat.Nom).ToList();

			// Puis pour la barre de recherche
			BoiteRechPlat = BoiteRechPlat.OrderBy(plat => plat.Nom).ToList();
			BoiteRechAliment = BoiteRechAliment.OrderBy(aliment => aliment.Nom).ToList();

			// On déssine la boite de recherche
			DessinerBoiteResultat();

			DessinerPlateau();

			Mouse.OverrideCursor = null;

			// --------- Entrée -------------
			FormerItemAccordeon("Entrée");

			// --------- Breuvage -------------
			FormerItemAccordeon("Breuvage");

			// --------- Plat principal -------------
			FormerItemAccordeon("Plat principal");

			// --------- Déssert -------------
			FormerItemAccordeon("Déssert");

			// --------- Déjeuner -------------
			FormerItemAccordeon("Déjeuner");
		}

		public void FormerItemAccordeon(string nomItem)
		{
			AccordionItem itemAccordeon = new AccordionItem();
			itemAccordeon.Header = nomItem;
			StackPanel stackLigne = new StackPanel();
			stackLigne.Background = Brushes.White;
			stackLigne.Width = 284;

			foreach (var plat in LstPlat)
			{
				Button btnPlat = FormerListeLignePlatAliment(true, plat, null);
				if (plat.TypePlat == nomItem)
					stackLigne.Children.Add(btnPlat);
			}

			itemAccordeon.Content = stackLigne;
			accPlat.Items.Add(itemAccordeon);
		}


		/// <summary>
		/// Méthode qui défini le filtre de recherche d'un Aliment pour SearchBox
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public void FiltrerChampRecherche(string champ)
		{
			BoiteRechAliment = LstAliment.FindAll(A => EnleverAccent(A.Nom).ToLower().Contains(EnleverAccent(champ).ToLower())).OrderBy(aliment => aliment.Nom).ToList();
			BoiteRechPlat = LstPlat.FindAll(P => EnleverAccent(P.Nom).ToLower().Contains(EnleverAccent(champ).ToLower())).OrderBy(plat => plat.Nom).ToList();
		}

		public string EnleverAccent(string text)
		{
			byte[] tempBytes;
			tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(text);
			string asciiStr = System.Text.Encoding.UTF8.GetString(tempBytes);
			return asciiStr;
		}

		/// <summary>
		/// Méthode qui génere deux StackPanel pour les plat et aliment dans le plateau
		/// </summary>
		private void DessinerPlateau()
		{
			double scrollActuel = 0;
			if (Plateau != null)
				scrollActuel = Plateau.svPlateau.VerticalOffset;
			Plateau = new SousEcran();
			presenteurContenu2.Content = Plateau;
			lstIdPresent = new List<int?>();
			if (Plateau != null)
				Plateau.svPlateau.ScrollToVerticalOffset(scrollActuel);

			foreach (var plat in PlateauPlat)
			{
				Button btnPlat = FormerListeLignePlatAliment(false, plat, lstIdPresent);

				if (btnPlat != null)
				{
					Plateau.stackEcran.Children.Add(btnPlat);
					if (LstIdPlatADerouler.Any(idP => idP == plat.IdPlat))
					{
						foreach (var aliment in plat.ListeIngredients)
						{
							Button btnAliment = FormerListeLignePlatAliment(null, aliment, null);
							Plateau.stackEcran.Children.Add(btnAliment);
						}
					}

				}
			}

			foreach (var aliment in PlateauAliment)
			{
				Button btnAliment = FormerListeLignePlatAliment(false, aliment, lstIdPresent);

				if (btnAliment != null)
					Plateau.stackEcran.Children.Add(btnAliment);
			}

			// On met à jour le tableau de valeur nutritionel
			CalculerValeurNutritionelle();

		}

		/// <summary>
		/// Méthode qui génere deux StackPanel pour les plat et aliment dans la boite de resultat de la barre de recherche
		/// </summary>
		private void DessinerBoiteResultat()
		{
			double scrollActuel = 0;
			if (BoiteResultat != null)
				scrollActuel = BoiteResultat.svPlateau.VerticalOffset;
			BoiteResultat = new SousEcran();
			presenteurContenu4.Content = BoiteResultat;
			if (BoiteResultat != null)
				BoiteResultat.svPlateau.ScrollToVerticalOffset(scrollActuel);

			foreach (var plat in BoiteRechPlat)
			{
				Button btnPlat = FormerListeLignePlatAliment(true, plat, null);

				if (btnPlat != null)
					BoiteResultat.stackEcran.Children.Add(btnPlat);
			}

			foreach (var aliment in BoiteRechAliment)
			{
				Button btnAliment = FormerListeLignePlatAliment(true, aliment, null);

				if (btnAliment != null)
					BoiteResultat.stackEcran.Children.Add(btnAliment);
			}
		}


		/// <summary>
		/// Ajoute la ligne de Plat si l'utilisateur clique sur le bouton
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AjoutItem_Click(object sender, RoutedEventArgs e)
		{
			Button btn = (Button)sender;
			int idObjet = Convert.ToInt32(btn.Uid);

			if (idObjet > 0)
			{
				foreach (var plat in LstPlat)
				{
					if (plat.IdPlat == idObjet)
					{
						int iteration = 1;
						if (Keyboard.Modifiers == ModifierKeys.Control)
							iteration = 10;

						int posPlatActuel = PlateauPlat.FindIndex(P => P == plat);

						for (int i = 0; i < iteration; i++)
						{
							PlateauPlat.Insert(posPlatActuel + 1, plat);
						}
					}
				}
			}
			else
			{
				foreach (var aliment in BoiteRechAliment)
				{
					if (aliment.IdAliment == idObjet * -1)
					{
						int iteration = 1;
						if (Keyboard.Modifiers == ModifierKeys.Control)
							iteration = 10;

						int posAlimentActuel = PlateauAliment.FindIndex(A => A == aliment);

						for (int i = 0; i < iteration; i++)
						{
							PlateauAliment.Insert(posAlimentActuel + 1, aliment);
						}
					}
				}
			}


			DessinerPlateau();
		}



		/// <summary>
		/// Génere une ligne de plat ou aliment 
		/// comportant son nom et son bouton pouvant l'Ajouter ou le supprimer
		/// </summary>
		/// <param name="plus">Parametre qui permet de determiner si la méthode
		/// est appelé pour faire une ligne avec un bouton plus ou un bouton moins</param>
		/// <param name="obj">Plat ou Aliment</param>
		/// <returns></returns>
		Button FormerListeLignePlatAliment(bool? plus, Object obj, List<int?> lstIdPresent)
		{

			// Les plats seront positif et les aliment, négatif

			Plat plat;
			Aliment aliment;
			Button btnControl = new Button();


			bool EstPlat = false;


			if (obj.GetType().ToString() == "Nutritia.Plat")
				EstPlat = true;

			// Si c'est un plat que l'on recoit, alors on définnit aliment a null et on crée une instance de plat
			// Sinon, le contraire
			plat = (EstPlat ? (Plat)obj : null);
			aliment = (!EstPlat ? (Aliment)obj : null);


			// Si la liste est null (On n'apelle pas cette fonction dans le contexte du plateau) alors on rentre dans le bloc
			// Sinon si la liste d'id ne contient pas déja l'id du plat actuelle, alors on rentre pour ajouter ce plat/aliment
			// Sinon, si la liste d'id contient déja l'id du plat actuelle, alors on ne rentre pas car on ne veut pas mettre 2 plats
			// Aussi : Pour les aliments, on utilise le contraire de son id afin de retrouver dans la liste la partie des aliments (id * -1)
			if (lstIdPresent != null ? (!lstIdPresent.Contains((EstPlat ? plat.IdPlat : aliment.IdAliment * -1))) : true)
			{
				// On a pas besoin d'ajouter l'id lorsque le contexte d'apelle n'est pas pour le plateau (Seul place avec un compteur)
				if (lstIdPresent != null)
				{
					// On définnit que l'on à dessiner le plat
					if (EstPlat)
						lstIdPresent.Add(plat.IdPlat);
					else
						lstIdPresent.Add(aliment.IdAliment * -1);
				}

				// Création du bouton pour supprimer ou ajouter un Plat/Aliment
				btnControl.HorizontalContentAlignment = HorizontalAlignment.Left;
				if (plus == null)
				{
					// On veut un margin pour décaller
					Thickness margin = btnControl.Margin;
					margin.Left = 20;
					btnControl.Margin = margin;

					// On ne veut pas pouvoir cliquer sur le bouton
					btnControl.IsEnabled = false;
					btnControl.SetValue(ToolTipService.ShowOnDisabledProperty, true);
				}

				btnControl.Height = 32;

				if (plus != null)
				{
					if ((bool)plus)
						btnControl.Click += AjoutItem_Click;
					else if ((bool)!plus)
					{
						btnControl.Click += BtnControlSupprimer_Click;
						btnControl.MouseRightButtonDown += BtnControlDerouler_Click;
					}

					btnControl.Uid = (EstPlat ? plat.IdPlat : aliment.IdAliment * -1).ToString();
					btnControl.Cursor = Cursors.Hand;
				}

				StackPanel stackLigne = new StackPanel();
				stackLigne.Orientation = Orientation.Horizontal;
				stackLigne.HorizontalAlignment = HorizontalAlignment.Left;
				stackLigne.Width = 275;
				if (plus != null)
				{
					// Image de bouton
					Image imgBouton = new Image();
					imgBouton.Source = new BitmapImage(new Uri("pack://application:,,,/UI/Images/" + ((bool)plus ? "plusIcon" : "minusIcon") + ".png"));
					imgBouton.Width = 15;
					imgBouton.Height = 15;
					stackLigne.Children.Add(imgBouton);
				}

				// Génération du Label comportant le nom du Plat/Aliment
				Label lblNom = new Label();
				lblNom.FontSize = 12;
				lblNom.Width = 230;

				if (EstPlat)
				{
					int nbrMemePlat = PlateauPlat.Count(x => x.IdPlat == plat.IdPlat);
					// Si on passe null a lstIdPresent, c'est qu'on ne veut pas reproduire ce plat
					if (lstIdPresent == null)
						nbrMemePlat = 0;
					lblNom.Content = (nbrMemePlat > 0 && lstIdPresent != null ? nbrMemePlat.ToString() + " " : "") + plat.Nom;
					stackLigne.Children.Add(lblNom);
				}
				else
				{
					int nbrMemeAliment = PlateauAliment.Count(x => x.IdAliment == aliment.IdAliment);
					if (lstIdPresent == null)
						nbrMemeAliment = 0;
					lblNom.Content = (nbrMemeAliment > 0 ? nbrMemeAliment.ToString() + " " : "") + aliment.Nom;
					stackLigne.Children.Add(lblNom);
				}

				// Image pour détérminer si c'est un plat ou un aliment
				Image imgTypeElement = new Image();
				imgTypeElement.Source = new BitmapImage(new Uri("pack://application:,,,/UI/Images/" + (EstPlat ? "PlatIcon" : "IngredientsIcon") + ".png"));

				imgTypeElement.Height = 15;
				stackLigne.Children.Add(imgTypeElement);

				// Insertion d'un hover tooltip sur le StackPanel
				if (EstPlat)
					btnControl.ToolTip = GenererToolTipValeursNutritive(plat);
				else
					btnControl.ToolTip = GenererToolTipValeursNutritive(aliment);

				btnControl.Content = stackLigne;

			}
			else
				btnControl = null;


			return btnControl;
		}

		/// <summary>
		/// Evenement qui ajoute un plat à dérouler dans la liste ou l'hôte de la liste
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnControlDerouler_Click(object sender, MouseButtonEventArgs e)
		{
			Button btnPlat = (Button)sender;

			int idPlatCourrant = Convert.ToInt32(btnPlat.Uid);
			if (Keyboard.Modifiers == ModifierKeys.Shift)
			{
				if (LstIdPlatADerouler.Count != 0)
					LstIdPlatADerouler.Clear();
				else
					foreach (var plat in PlateauPlat)
						LstIdPlatADerouler.Add((int)plat.IdPlat);
			}

			else
			{
				if (LstIdPlatADerouler.Any(idP => idP == idPlatCourrant))
					LstIdPlatADerouler.Remove(idPlatCourrant);
				else
					LstIdPlatADerouler.Add(idPlatCourrant);
			}

			DessinerPlateau();
		}


		/// <summary>
		/// Supprime la ligne de Plat/Aliment si l'utilisateur clique sur le bouton
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnControlSupprimer_Click(object sender, RoutedEventArgs e)
		{
			Button btn = (Button)sender;
			Plat monPlat = new Plat();
			Aliment monAliment = new Aliment();

			bool estPlat = Convert.ToInt32(btn.Uid) > 0;

			// On compte le nbr de plat ou aliment présent dans le plateau pour controller sa suppréssion
			int nbrItemActuel = (estPlat ?
								PlateauPlat.Count(P => P.IdPlat == Convert.ToInt32(btn.Uid)) :
								PlateauAliment.Count(A => A.IdAliment == Convert.ToInt32(btn.Uid) * -1));
			int iteration = 1;
			if (Keyboard.Modifiers == ModifierKeys.Control)
				iteration = (nbrItemActuel < 10 ?
							nbrItemActuel :
							10);

			else if (Keyboard.Modifiers == ModifierKeys.Shift)
				iteration = nbrItemActuel;


			// On supprime 1 / 10 / ou tous le items qui correspondent à l'objet cliqué
			if (estPlat)
			{
				// Si on vide ce plat, il faut enlever son id de la liste des plats à dérouler
				if (nbrItemActuel == iteration)
					LstIdPlatADerouler.Remove(Convert.ToInt32(btn.Uid));
				for (int i = 0; i < iteration; i++)
					PlateauPlat.Remove(PlateauPlat.Last(P => P.IdPlat == Convert.ToInt32(btn.Uid)));

			}


			else
				for (int i = 0; i < iteration; i++)
					PlateauAliment.Remove(PlateauAliment.Last(P => P.IdAliment == Convert.ToInt32(btn.Uid) * -1));

			DessinerPlateau();
		}



		/// <summary>
		/// Méthode permettant de générer les valeurs nutritionnelles d'un aliment dans un tooltip. (Prise de Guillaume)
		/// </summary>
		/// <param name="item">Un aliment ou un plat dépendant du contexte de l'apelle</param>
		/// <returns>Un tooltip contenant les valeurs nutritionnelles de l'aliment.</returns>
		private ToolTip GenererToolTipValeursNutritive(object item)
		{

			Plat plat = new Plat(); // Assignation temporaire ...
			if (item is Plat)
			{
				plat = (Plat)item;
			}

			else // On cast l'aliment dans la liste de notre plat vide
			{

				plat.ListeIngredients = new List<Aliment>();
				plat.ListeIngredients.Add((Aliment)item);

			}

			// Construction du Tooltip

			ToolTip ttValeurNut = new ToolTip();
			StackPanel spValeurNut = new StackPanel();

			Label lblEntete = new Label();
			lblEntete.Content = "Valeurs nutritive";
			spValeurNut.Children.Add(lblEntete);

			ValeurNutritive = new Dictionary<string, double>();

			ValeurNutritive = ConstruireDicValeurNutritive(null, ValeurNutritive);

			int poidPlat = 0;

			foreach (var aliment in plat.ListeIngredients)
			{
				ValeurNutritive = ConstruireDicValeurNutritive(aliment, ValeurNutritive);
				poidPlat += aliment.Mesure;
			}

			StringBuilder sbValeurNut = new StringBuilder();
			sbValeurNut.Append("1 ").Append(item is Plat ? plat.Nom : plat.ListeIngredients[0].Nom).AppendLine(" de " + poidPlat + " g").AppendLine(); // Affichage du nom du plat ou de l'aliment
			sbValeurNut.Append("Énergie : ").Append(ValeurNutritive["Calorie"].ToString("N")).AppendLine(" cal");
			sbValeurNut.Append("Glucides : ").Append(ValeurNutritive["Glucides"].ToString("N")).AppendLine(" g");
			sbValeurNut.Append("Fibres : ").Append(ValeurNutritive["Fibres"].ToString("N")).AppendLine(" g");
			sbValeurNut.Append("Protéines : ").Append(ValeurNutritive["Proteines"].ToString("N")).AppendLine(" g");
			sbValeurNut.Append("Lipides : ").Append(ValeurNutritive["Lipides"].ToString("N")).AppendLine(" g");
			sbValeurNut.Append("Cholestérol : ").Append(ValeurNutritive["Cholesterol"].ToString("N")).AppendLine(" mg");
			sbValeurNut.Append("Sodium : ").Append(ValeurNutritive["Sodium"].ToString("N")).Append(" mg");
			Label lblValeurNut = new Label();
			lblValeurNut.Content = sbValeurNut.ToString();

			spValeurNut.Children.Add(lblValeurNut);
			ttValeurNut.Content = spValeurNut;
			return ttValeurNut;
		}

		/// <summary>
		/// Fait le calcul de toute les valeurs nutritionelles de chaque aliments et de chaque plats (Incluant le compteur)
		/// </summary>
		void CalculerValeurNutritionelle()
		{
			ValeurNutritive = new Dictionary<string, double>();

			ValeurNutritive = ConstruireDicValeurNutritive(null, ValeurNutritive);

			foreach (var Plat in PlateauPlat)
			{
				if (Plat.ListeIngredients != null)
					foreach (var aliment in Plat.ListeIngredients)
						ValeurNutritive = ConstruireDicValeurNutritive(aliment, ValeurNutritive);
			}

			foreach (var aliment in PlateauAliment)
				ValeurNutritive = ConstruireDicValeurNutritive(aliment, ValeurNutritive);

			DessinerTabValeurNutritionelle();

		}

		/// <summary>
		/// Formate l'écran des valeurs nutritionnelles
		/// </summary>
		void DessinerTabValeurNutritionelle()
		{
			TabValeurNutritionelle = new SousEcran2();
			presenteurContenu3.Content = TabValeurNutritionelle;

			TabValeurNutritionelle.vEnegie.Inlines.Add(Math.Round(ValeurNutritive["Calorie"], 2).ToString("N"));
			TabValeurNutritionelle.vGlucide.Inlines.Add(Math.Round(ValeurNutritive["Glucides"], 2).ToString("N"));
			TabValeurNutritionelle.vFibre.Inlines.Add(Math.Round(ValeurNutritive["Fibres"], 2).ToString("N"));
			TabValeurNutritionelle.vProtein.Inlines.Add(Math.Round(ValeurNutritive["Proteines"], 2).ToString("N"));
			TabValeurNutritionelle.vLipide.Inlines.Add(Math.Round(ValeurNutritive["Lipides"], 2).ToString("N"));
			TabValeurNutritionelle.vCholesterol.Inlines.Add(Math.Round(ValeurNutritive["Cholesterol"], 2).ToString("N"));
			TabValeurNutritionelle.vSodium.Inlines.Add(Math.Round(ValeurNutritive["Sodium"], 2).ToString("N"));

		}

		/// <summary>
		/// Constructeur et remplisseur d'un dictionnaire de donnée concernant les valeurs nutritionnelles
		/// </summary>
		/// <param name="aliment"></param>
		/// <param name="dValeurNutritive"></param>
		/// <returns></returns>
		Dictionary<String, Double> ConstruireDicValeurNutritive(Aliment aliment, Dictionary<String, Double> dValeurNutritive)
		{
			if (dValeurNutritive.Count == 0)
			{
				dValeurNutritive.Add("Calorie", 0);
				dValeurNutritive.Add("Glucides", 0);
				dValeurNutritive.Add("Fibres", 0);
				dValeurNutritive.Add("Proteines", 0);
				dValeurNutritive.Add("Lipides", 0);
				dValeurNutritive.Add("Cholesterol", 0);
				dValeurNutritive.Add("Sodium", 0);
			}
			else
			{
				double mesure = aliment.Mesure;
				double quantite = aliment.Quantite;
				if (quantite / mesure <= 1) // Cas ou c'Est un simple aliment atomique qui est calculé et pas un plat
				{
					mesure = 1;
					quantite = 1;
				}

				dValeurNutritive["Calorie"] += aliment.Energie * quantite / mesure;
				dValeurNutritive["Glucides"] += aliment.Glucide * quantite / mesure;
				dValeurNutritive["Fibres"] += aliment.Fibre * quantite / mesure;
				dValeurNutritive["Proteines"] += aliment.Proteine * quantite / mesure;
				dValeurNutritive["Lipides"] += aliment.Lipide * quantite / mesure;
				dValeurNutritive["Cholesterol"] += aliment.Cholesterol * quantite / mesure;
				dValeurNutritive["Sodium"] += aliment.Sodium * quantite / mesure;
			}

			return dValeurNutritive;
		}

		/// <summary>
		/// Evenement qui Appelle des méthode pour mettre à jour la boite de 
		/// recherche en fonction du texte écrit par l'utilisateur
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void txtRecherche_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			FiltrerChampRecherche(textBox.Text.ToString());

			DessinerBoiteResultat();

		}

		/// <summary>
		/// Code de Guillaume (légerement modifié pour qu'il soit compatible avec un apel de tous les SV):
		/// Événement lancé lorsque la roulette de la souris est utilisée dans le "scrollviewer" contenant le menu.
		/// Explicitement, cet événement permet de gérer le "scroll" avec la roulette correctement sur toute la surface du "scrollviewer".
		/// Si on ne le gère pas, il est seulement possible de "scroller" lorsque le pointeur de la souris est situé sur la "scrollbar".
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ScrollFocus(object sender, MouseWheelEventArgs e)
		{
			ScrollViewer scrollViewer = (ScrollViewer)sender;
			scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
		}

		private void UserControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
				if (Keyboard.Modifiers == ModifierKeys.Control)
				{
					PlateauAliment.Clear();
					PlateauPlat.Clear();
					LstIdPlatADerouler.Clear();
					DessinerPlateau();
				}
		}

		private void btnVider_Click(object sender, RoutedEventArgs e)
		{
			PlateauAliment.Clear();
			PlateauPlat.Clear();
			LstIdPlatADerouler.Clear();
			DessinerPlateau();

		}

        private void CultureManager_UICultureChanged(object sender, EventArgs e)
        {
            App.Current.MainWindow.Title = FenetreCalculatriceNutritionnelle.Titre;
        }

	}


}
