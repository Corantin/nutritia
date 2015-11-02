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

        private List<int?> lstIdPresent { get; set; }
        private SousEcran Plateau { get; set; }
        private SousEcran2 TabValeurNutritionelle { get; set; }



        public FenetreCalculatriceNutritionelle()
        {
            InitializeComponent();

            Plateau = new SousEcran();
            presenteurContenu2.Content = Plateau;
            TabValeurNutritionelle = new SousEcran2();
            presenteurContenu3.Content = TabValeurNutritionelle;
            LstPlat = new List<Plat>();
            LstAliment = new List<Aliment>();
            PlateauPlat = new List<Plat>();
            PlateauAliment = new List<Aliment>();
            ValeurNutritive = new Dictionary<string, double>();

            // On génere l'écran des valeurs nutritives
            CalculerValeurNutritionelle();

            Mouse.OverrideCursor = Cursors.Wait;

            LstPlat.AddRange(ServiceFactory.Instance.GetService<IPlatService>().RetrieveAll());
            LstAliment.AddRange(ServiceFactory.Instance.GetService<IAlimentService>().RetrieveAll());

            Mouse.OverrideCursor = null;

            // --------- Entrée -------------
            AccordionItem itemEntree = new AccordionItem();
            itemEntree.Header = "Entrée";
            StackPanel stackEntree = new StackPanel();
            stackEntree.Background = Brushes.White;
            stackEntree.Width = 284;

            // --------- Breuvage -------------
            AccordionItem itemBreuvage = new AccordionItem();
            itemBreuvage.Header = "Breuvage";
            StackPanel stackBreuvage = new StackPanel();
            stackBreuvage.Background = Brushes.White;
            stackBreuvage.Width = 284;

            // --------- Plat principal -------------
            AccordionItem itemPlatPrincipal = new AccordionItem();
            itemPlatPrincipal.Header = "Plat principaux";
            StackPanel stackPlatPrincipal = new StackPanel();
            stackPlatPrincipal.Background = Brushes.White;
            stackPlatPrincipal.Width = 284;

            // --------- Déssert -------------
            AccordionItem itemDessert = new AccordionItem();
            itemDessert.Header = "Déssert";
            StackPanel stackDessert = new StackPanel();
            stackDessert.Background = Brushes.White;
            stackDessert.Width = 284;

            // --------- Déjeuner -------------
            AccordionItem itemDejeuner = new AccordionItem();
            itemDejeuner.Header = "Déjeuner";
            StackPanel stackDejeuner = new StackPanel();
            stackDejeuner.Background = Brushes.White;
            stackDejeuner.Width = 284;

            // Dispatch des aliment trié par nom dans les différentes parties de l'accordéon
            LstPlat = LstPlat.OrderBy(plat => plat.Nom).ToList();


            foreach (var plat in LstPlat)
            {

                Button btnPlat = FormerListeLignePlatAliment(true, plat, null);

                if (plat.TypePlat == "Entrée")
                    stackEntree.Children.Add(btnPlat);
                else if (plat.TypePlat == "Breuvage")
                    stackBreuvage.Children.Add(btnPlat);
                else if (plat.TypePlat == "Plat principal")
                    stackPlatPrincipal.Children.Add(btnPlat);
                else if (plat.TypePlat == "Déssert")
                    stackDessert.Children.Add(btnPlat);
                else if (plat.TypePlat == "Déjeuner")
                    stackDejeuner.Children.Add(btnPlat);
            }

            itemDejeuner.Content = stackDejeuner;
            accPlat.Items.Add(itemDejeuner);

            itemBreuvage.Content = stackBreuvage;
            accPlat.Items.Add(itemBreuvage);

            itemEntree.Content = stackEntree;
            accPlat.Items.Add(itemEntree);

            itemPlatPrincipal.Content = stackPlatPrincipal;
            accPlat.Items.Add(itemPlatPrincipal);

            itemDessert.Content = stackDessert;
            accPlat.Items.Add(itemDessert);
            
            //Setting de dgRecherche

            dgRecherche.DataGridCollection = CollectionViewSource.GetDefaultView(ServiceFactory.Instance.GetService<IAlimentService>().RetrieveAll());
            dgRecherche.DataGridCollection.Filter = new Predicate<object>(Filter);
            DataGrid teste = dgRecherche.FindName("dgResults") as DataGrid;
            List<object> listObject = new List<object>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(teste); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(teste, i);
                if (child != null)
                {
                    listObject.Add(child);
                }
            }
            //listBtn.ForEach(x => x.ToolTip = GenererToolTipValeursNutritive(AlimentFromDataContext(x)));
        }


        /// <summary>
        /// Méthode qui défini le filtre de recherche d'un Aliment pour SearchBox
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Filter(object obj)
        {
            var data = obj as Aliment;
            if (data != null)
            {
                if (!string.IsNullOrEmpty(dgRecherche.FilterString))
                {
                    return dgRecherche.Filter(data.Nom);
                }
                return true;
            }
            return false;
        }


        /// <summary>
        /// Méthode qui génere deux StackPanel pour les plat et aliment dans le plateau
        /// </summary>
        private void DessinerPlateau()
        {
            Plateau = new SousEcran();
            presenteurContenu2.Content = Plateau;
            lstIdPresent = new List<int?>();

            foreach (var plat in PlateauPlat)
            {
                Button btnPlat = FormerListeLignePlatAliment(false, plat, lstIdPresent);

                if (btnPlat != null)
                    Plateau.stackEcran.Children.Add(btnPlat);
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
        /// Evenement d'ajout d'un aliment au plateau pour la barre de recherche
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                Button btn = sender as Button;
                Aliment aliment = AlimentFromDataContext(btn);
                if (aliment != null)
                {
                    PlateauAliment.Add(aliment);
                    DessinerPlateau();
                }
            }
        }

        /// <summary>
        /// Supprime la ligne de Plat/Aliment si l'utilisateur clique sur le bouton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnControlSupprimer_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            if (Convert.ToInt32(btn.Uid) > 0)
                PlateauPlat.Remove(LstPlat.First(plat => plat.IdPlat == Convert.ToInt32(btn.Uid)));
            else
                PlateauAliment.Remove(LstAliment.First(aliment => aliment.IdAliment == Convert.ToInt32(btn.Uid) * -1));

            DessinerPlateau();
        }

        /// <summary>
        /// Ajoute la ligne de Plat/Aliment si l'utilisateur clique sur le bouton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnControlAjout_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            foreach (var plat in LstPlat)
            {
                if (plat.IdPlat == Convert.ToInt32(btn.Uid))
                    PlateauPlat.Add(plat);
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
        Button FormerListeLignePlatAliment(bool plus, Object obj, List<int?> lstIdPresent)
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
                Thickness margin = btnControl.Margin;
                margin.Left = 0;
                btnControl.Margin = margin;
                btnControl.Height = 32;

                if (plus)
                    btnControl.Click += BtnControlAjout_Click;
                else
                    btnControl.Click += BtnControlSupprimer_Click;
                btnControl.Uid = (EstPlat ? plat.IdPlat : aliment.IdAliment * -1).ToString();
                btnControl.Cursor = Cursors.Hand;


                StackPanel stackLigne = new StackPanel();
                stackLigne.Orientation = Orientation.Horizontal;
                stackLigne.HorizontalAlignment = HorizontalAlignment.Left;
                stackLigne.Width = 275;
                // Image de bouton
                Image imgBouton = new Image();
                imgBouton.Source = new BitmapImage(new Uri("pack://application:,,,/UI/Images/" + (plus ? "plusIcon" : "minusIcon") + ".png"));
                imgBouton.Width = 15;
                imgBouton.Height = 15;
                stackLigne.Children.Add(imgBouton);

                // Génération du Label comportant le nom du Plat/Aliment
                Label lblNom = new Label();
                lblNom.Style = (Style)(this.Resources["fontNutitia"]);
                lblNom.FontSize = 12;

                //Compte le nombre d'item dans la liste qui correspondent à la condition
                if (EstPlat)
                {
                    int nbrMemePlat = PlateauPlat.Count(x => x.IdPlat == plat.IdPlat);
                    lblNom.Content = (nbrMemePlat > 0 ? nbrMemePlat.ToString() + " " : "") + plat.Nom;
                    stackLigne.Children.Add(lblNom);
                }
                else
                {
                    int nbrMemeAliment = PlateauAliment.Count(x => x.IdAliment == aliment.IdAliment);
                    lblNom.Content = (nbrMemeAliment > 0 ? nbrMemeAliment.ToString() + " " : "") + aliment.Nom;
                    stackLigne.Children.Add(lblNom);
                }


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
        /// Supprime la ligne de Plat/Aliment si l'utilisateur clique sur le bouton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnControlSupprimer_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Plat monPlat = new Plat();
            Aliment monAliment = new Aliment();

            int iteration = 1;
            if (Keyboard.Modifiers == ModifierKeys.Control)
                iteration = 10;
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
                iteration = (Convert.ToInt32(btn.Uid) > 0 ?
                            PlateauPlat.Count :
                            PlateauAliment.Count);

            if (Convert.ToInt32(btn.Uid) > 0)
            {
                foreach (var plat in PlateauPlat)
                    if (plat.IdPlat == Convert.ToInt32(btn.Uid))
                    {
                        monPlat = plat;
                        break;
                    }

                PlateauPlat.RemoveRange(0, iteration);
            }

            else
            {

                foreach (var aliment in PlateauAliment)
                    if (aliment.IdAliment == Convert.ToInt32(btn.Uid) * -1)
                    {
                        monAliment = aliment;
                        break;
                    }

                PlateauAliment.RemoveRange(0, iteration);
            }


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
            bool EstPlat = false;
            if (item.GetType().ToString() == "Nutritia.Plat")
            {
                plat = (Plat)item;
                EstPlat = true;
            }

            if (!EstPlat) // On cast l'aliment dans la liste de notre plat vide
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

            foreach (var aliment in plat.ListeIngredients)
                ValeurNutritive = ConstruireDicValeurNutritive(aliment, ValeurNutritive);


            StringBuilder sbValeurNut = new StringBuilder();
            sbValeurNut.Append("1 ").AppendLine(EstPlat ? plat.Nom : plat.ListeIngredients[0].Nom).AppendLine(); // Affichage du nom du plat ou de l'aliment
            sbValeurNut.Append("Énergie : ").Append(ValeurNutritive["Calorie"]).AppendLine(" cal");
            sbValeurNut.Append("Glucides : ").Append(ValeurNutritive["Glucides"]).AppendLine(" g");
            sbValeurNut.Append("Fibres : ").Append(ValeurNutritive["Fibres"]).AppendLine(" g");
            sbValeurNut.Append("Protéines : ").Append(ValeurNutritive["Proteines"]).AppendLine(" g");
            sbValeurNut.Append("Lipides : ").Append(ValeurNutritive["Lipides"]).AppendLine(" g");
            sbValeurNut.Append("Cholestérol : ").Append(ValeurNutritive["Cholesterol"]).AppendLine(" mg");
            sbValeurNut.Append("Sodium : ").Append(ValeurNutritive["Sodium"]).Append(" mg");
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

            TabValeurNutritionelle.vEnegie.Inlines.Add(Math.Round(ValeurNutritive["Calorie"], 2).ToString("###########0.0"));
            TabValeurNutritionelle.vGlucide.Inlines.Add(Math.Round(ValeurNutritive["Glucides"], 2).ToString("############0.0"));
            TabValeurNutritionelle.vFibre.Inlines.Add(Math.Round(ValeurNutritive["Fibres"], 2).ToString("############0.0"));
            TabValeurNutritionelle.vProtein.Inlines.Add(Math.Round(ValeurNutritive["Proteines"], 2).ToString("############0.0"));
            TabValeurNutritionelle.vLipide.Inlines.Add(Math.Round(ValeurNutritive["Lipides"], 2).ToString("############0.0"));
            TabValeurNutritionelle.vCholesterol.Inlines.Add(Math.Round(ValeurNutritive["Cholesterol"], 2).ToString("############0.0"));
            TabValeurNutritionelle.vSodium.Inlines.Add(Math.Round(ValeurNutritive["Sodium"], 2).ToString("############0.0"));

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
                dValeurNutritive["Calorie"] += aliment.Energie; //* aliment.Quantite;
                dValeurNutritive["Glucides"] += aliment.Glucide; //* aliment.Quantite;
                dValeurNutritive["Fibres"] += aliment.Fibre; // * aliment.Quantite;
                dValeurNutritive["Proteines"] += aliment.Proteine; // * aliment.Quantite;
                dValeurNutritive["Lipides"] += aliment.Lipide; // * aliment.Quantite;
                dValeurNutritive["Cholesterol"] += aliment.Cholesterol; // * aliment.Quantite;
                dValeurNutritive["Sodium"] += aliment.Sodium; // * aliment.Quantite;
            }

            return dValeurNutritive;
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

        private Aliment AlimentFromDataContext(Button box)
        {
            var context = box.DataContext;
            if (context is Aliment)
            {
                return context as Aliment;
            }
            return null;
        }



    }




}
