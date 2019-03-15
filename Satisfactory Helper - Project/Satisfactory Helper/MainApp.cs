using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Satisfactory_Helper
{
    public partial class MainApp : Form
    {
        public MainApp()
        {
            InitializeComponent();
        }

        //keeps track of if the calculate button has been pressed before
        public bool runPrev = false;

        //make lists for all displays
        List<Label> itemsNeededLabels = new List<Label>();
        List<Label> buildingsNeededLabels = new List<Label>();
        List<PictureBox> itemsNeededImages = new List<PictureBox>();
        List<PictureBox> buildingsNeededImages = new List<PictureBox>();


        private void calcButton_Click(object sender, EventArgs e)
        {
            runPrev = true;
            //initialize list of values and set equal to user input
            List<Double> amountPerMin = new List<Double>();
            List<string> textBoxes = new List<string> {
                amountPerMinBox1.Text,
                amountPerMinBox2.Text,
                amountPerMinBox3.Text,
                amountPerMinBox4.Text,
                amountPerMinBox5.Text,
                amountPerMinBox6.Text,
                amountPerMinBox7.Text,
                amountPerMinBox8.Text,
                amountPerMinBox9.Text,
                amountPerMinBox10.Text
            };
            for(int i = 0; i < 10; i++)
            {
                if (!string.IsNullOrEmpty(textBoxes.ElementAt(i)))
                {
                    //user entered something in textbox i so get it
                    amountPerMin.Add(Convert.ToDouble(textBoxes.ElementAt(i)));
                }
                else
                {
                    //nothing was entered in textbox i so set equal to zero
                    amountPerMin.Add(0);
                }
            }

            List<item> items = new List<item>();
            List<string> optionBoxes = new List<string>
            {
                itemOptionBox1.Text,
                itemOptionBox2.Text,
                itemOptionBox3.Text,
                itemOptionBox4.Text,
                itemOptionBox5.Text,
                itemOptionBox6.Text,
                itemOptionBox7.Text,
                itemOptionBox8.Text,
                itemOptionBox9.Text,
                itemOptionBox10.Text,
            };
            for (int i = 0; i < 10; i++)
            {
                if (!string.IsNullOrEmpty(optionBoxes.ElementAt(i)))
                {
                    //user selected somthing in dropdown
                    items.Add(new item(optionBoxes.ElementAt(i)));
                }
                else
                {
                    //nothing was selected
                    items.Add(new item("Not Specified"));
                }
            }

            //perform calculations
            List<ResourceRate> totalResourcePerMin = new List<ResourceRate>();
            //List<resource> totalOverhead = new List<resource>();    //CHANGE THIS FOR OVERCLOCKING
            for(int i = 0; i < items.Count; i++)
            {
                //make sure the user entered an amount for this item (non-negative)
                if(amountPerMin.ElementAt(i) > 0)
                {
                    //get the ratio of deisred production:base production
                    double baseRate = items.ElementAt(i).getBaseProductionRate();
                    double requestedRate = amountPerMin.ElementAt(i);
                    double productionRatio = requestedRate / baseRate;
                    //int multiplier = Convert.ToInt32(Math.Ceiling(productionRatio));    //CHANGE THIS FOR OVERCLOCKING

                    //readjust the amout of materials used in the process for each resource
                    List<resource> tempResources = items.ElementAt(i).getResourcesNeeded();
                    List<ResourceRate> tempResourcePerMin = new List<ResourceRate>();
                    for (int j = 0; j < tempResources.Count; j++)
                    {
                        int tempAmount = tempResources.ElementAt(j).getAmount();
                        //get the base resource per min required to run the base process
                        double baseAmountPerMin = tempAmount * baseRate;
                        double neededAmountPerMin = baseAmountPerMin * productionRatio;

                        if(tempResourcePerMin.Count == 0)
                        {
                            //if the list is empty we can just add this resource into the list
                            tempResourcePerMin.Add(new ResourceRate(tempResources.ElementAt(j).getName(), neededAmountPerMin));
                        }
                        else
                        {
                            //otherwise we need to check if there are any resources of this type in the list yet
                            bool isInList = false;
                            for(int k = 0; k < tempResourcePerMin.Count; k++)
                            {
                                if (tempResourcePerMin.ElementAt(k).getName().Equals(tempResources.ElementAt(j).getName()))
                                {
                                    isInList = true;
                                    tempResourcePerMin.ElementAt(k).setRate(tempResourcePerMin.ElementAt(k).getRate() + neededAmountPerMin);
                                    break;
                                }
                            }
                            if (!isInList)
                            {
                                tempResourcePerMin.Add(new ResourceRate(tempResources.ElementAt(j).getName(), neededAmountPerMin));
                            }
                        }
                    }

                    //now add resources per minute to the total
                    //need to check if the list is empty, or if there are any existing members with the names in tempResourcePerMin
                    if(totalResourcePerMin.Count == 0)
                    {
                        //list is empty
                        totalResourcePerMin = tempResourcePerMin;
                    }
                    else
                    {
                        for(int m = 0; m < tempResourcePerMin.Count; m++)
                        {
                            bool isInList = false;
                            for (int n = 0; n < totalResourcePerMin.Count; n++)
                            {
                                //check if the resource exists in the list yet
                                if (totalResourcePerMin.ElementAt(n).getName().Equals(tempResourcePerMin.ElementAt(m).getName()))
                                {
                                    isInList = true;
                                    totalResourcePerMin.ElementAt(n).setRate(
                                        totalResourcePerMin.ElementAt(n).getRate() + tempResourcePerMin.ElementAt(m).getRate());
                                    break;
                                }
                            }
                            if (!isInList)
                            {
                                //doesnt exist yet so add it
                                totalResourcePerMin.Add(tempResourcePerMin.ElementAt(m));
                            }
                        }
                    }
                    ResourceRate topLevelResource = new ResourceRate(items.ElementAt(i).getName(), requestedRate);
                    bool isInList2 = false;
                    for (int n = 0; n < totalResourcePerMin.Count; n++)
                    {
                        //check if the resource exists in the list yet
                        if (totalResourcePerMin.ElementAt(n).getName().Equals(topLevelResource.getName()))
                        {
                            isInList2 = true;
                            totalResourcePerMin.ElementAt(n).setRate(
                                totalResourcePerMin.ElementAt(n).getRate() + topLevelResource.getRate());
                            break;
                        }
                    }
                    if (!isInList2)
                    {
                        //doesnt exist yet so add it
                        totalResourcePerMin.Add(topLevelResource);
                    }
                }
            }

            //making new item labels and images
            if (runPrev)
            {
                //clear items and images from prev run
                for(int i = 0; i < itemsNeededLabels.Count; i++)
                {
                    Controls.Remove(itemsNeededLabels.ElementAt(i));
                    itemsNeededLabels.Remove(itemsNeededLabels.ElementAt(i));
                }
                for (int i = 0; i < itemsNeededImages.Count; i++)
                {
                    Controls.Remove(itemsNeededImages.ElementAt(i));
                    itemsNeededImages.Remove(itemsNeededImages.ElementAt(i));
                }
            }
            
            for (int i = 0; i < totalResourcePerMin.Count; i++)
            {
                //make the label for the item and add to list
                Label tempLabel = new Label();
                tempLabel.AutoSize = true;
                tempLabel.BackColor = Color.Transparent;
                tempLabel.ForeColor = Color.White;
                tempLabel.Location = new Point((120 * (i%4)) + 10, (120 * (int)Math.Floor((double)(i/4))) + 100);
                tempLabel.MaximumSize = new Size(118, 40);
                tempLabel.Name = "inputLabel" + Convert.ToString(i);
                tempLabel.Size = new Size(36, 20);
                tempLabel.Text = totalResourcePerMin.ElementAt(i).getRate() + " " + totalResourcePerMin.ElementAt(i).getName() + "/min";
                tempLabel.TabStop = false;
                itemsNeededLabels.Add(tempLabel);

                //add to the panel space
                itemsPanel.Controls.Add(tempLabel);
                tempLabel.BringToFront();

                //make image for the item
                PictureBox tempPicBox = new PictureBox();
                tempPicBox.BackColor = Color.Transparent;
                tempPicBox.Location = new Point((120 * (i%4)) + 10, (120 * (int)Math.Floor((double)(i/4))) + 10);
                tempPicBox.Name = "itemPictureBox" + Convert.ToString(i);
                tempPicBox.Size = new Size(75, 75);
                tempPicBox.TabStop = false;
                tempPicBox.Image = totalResourcePerMin.ElementAt(i).getImage();
                itemsNeededImages.Add(tempPicBox);

                //add to the panel space
                itemsPanel.Controls.Add(tempPicBox);
                tempPicBox.BringToFront();
            }

            //now we have all the resource rates needed to make the requested item rate
            //calculate the production needed to make that happen

            //make a list to hold all the builds that will be needed
            List<building> buildings = new List<building>();
            //List<generator> generators = new List<generator>();
            double totalPowerConsumption = 0;

            //get the number of buildings needed to meet the item rate
            for (int i = 0; i < totalResourcePerMin.Count; i++)
            {
                string itemName = totalResourcePerMin.ElementAt(i).getName();
                item Item = new item(itemName);

                int numMachinesNeeded = (int)(totalResourcePerMin.ElementAt(i).getRate() / Item.getBaseProductionRate());
                string machineName = Item.getCraftIn();
                double lastMachineRate = 0;
                double lastMachineUnderclock = 0;
                bool uneven = false;    //set to true if the item rate isnt evenly divisable by the baserate
                if (totalResourcePerMin.ElementAt(i).getRate() % Item.getBaseProductionRate() != 0)
                {
                    uneven = true;
                    numMachinesNeeded += 1;
                    lastMachineRate = totalResourcePerMin.ElementAt(i).getRate() % Item.getBaseProductionRate();
                    lastMachineUnderclock = lastMachineRate / Item.getBaseProductionRate();
                }
                for(int j = 0; j < numMachinesNeeded; j++)
                {
                    buildings.Add(new building(Item.getCraftIn()));
                    buildings.ElementAt(j).setProducing(Item.getName());
                    if(j == numMachinesNeeded - 1 && uneven)
                    {
                        //we are at the last item so set the underclock rate
                        buildings.ElementAt(j).applyClock(lastMachineUnderclock);
                    }
                }
            }
            for(int i = 0; i < buildings.Count; i++)
            {
                //calculate total power consumption from all machines
                totalPowerConsumption += buildings.ElementAt(i).getPowerUsage();
            }
            string powerLabel = String.Format("{0:0.###}", totalPowerConsumption);
            powerUsageLabel.Text = powerLabel + " MW";

            //make a list of amount of all machines
            List<buildingCount> buildingCounts = new List<buildingCount>();
            for (int i = 0; i < buildings.Count; i++)
            {
                if(buildingCounts.Count == 0)
                {
                    buildingCounts.Add(new buildingCount(buildings.ElementAt(i).getName()));
                }
                else
                {
                    bool inList = false;
                    for(int j = 0; j < buildingCounts.Count; j++)
                    {
                        if (buildings.ElementAt(i).getName().Equals(buildingCounts.ElementAt(j).getName()))
                        {
                            inList = true;
                            buildingCounts.ElementAt(j).setAmount(buildingCounts.ElementAt(j).getAmount() + 1);
                            break;
                        }
                    }
                    if (!inList)
                    {
                        buildingCounts.Add(new buildingCount(buildings.ElementAt(i).getName()));
                    }
                }
            }

            //place images and labels for all buildings in buildingCounts
            if (runPrev)
            {
                //clear items and images from prev run
                for (int i = 0; i < buildingsNeededLabels.Count; i++)
                {
                    Controls.Remove(buildingsNeededLabels.ElementAt(i));
                    itemsNeededLabels.Remove(buildingsNeededLabels.ElementAt(i));
                }
                for (int i = 0; i < buildingsNeededImages.Count; i++)
                {
                    Controls.Remove(buildingsNeededImages.ElementAt(i));
                    buildingsNeededImages.Remove(buildingsNeededImages.ElementAt(i));
                }
            }

            for (int i = 0; i < buildingCounts.Count; i++)
            {
                //make the label for the building and add to list
                Label tempLabel = new Label();
                tempLabel.AutoSize = true;
                tempLabel.BackColor = Color.Transparent;
                tempLabel.ForeColor = Color.White;
                tempLabel.Location = new Point(120 * (i % 4) + 10, (120 * (int)Math.Floor((double)(i / 4))) + 100);
                tempLabel.MaximumSize = new Size(118, 40);
                tempLabel.Name = "buildingLabel" + Convert.ToString(i);
                tempLabel.Size = new System.Drawing.Size(36, 20);
                tempLabel.Text = buildingCounts.ElementAt(i).getAmount() + " " + buildingCounts.ElementAt(i).getName();
                tempLabel.TabStop = false;
                tempLabel.Visible = true;
                buildingsNeededLabels.Add(tempLabel);

                //add to the panel space
                buildingsPanel.Controls.Add(tempLabel);
                tempLabel.BringToFront();

                //make image for the building
                PictureBox tempPicBox = new PictureBox();
                tempPicBox.BackColor = Color.Transparent;
                tempPicBox.Location = new Point((120 * (i % 4)) + 10, (120 * (int)Math.Floor((double)(i/4))) + 10);
                tempPicBox.Name = "buildingPictureBox" + Convert.ToString(i);
                tempPicBox.Size = new Size(75, 75);
                tempPicBox.TabStop = false;
                tempPicBox.Visible = true;
                tempPicBox.Image = buildingCounts.ElementAt(i).getImage();
                buildingsNeededImages.Add(tempPicBox);

                //add button/hover functionality to the image
                tempPicBox.Click += new EventHandler(buildingImage_Click);
                MachineHoverToolTip.SetToolTip(tempPicBox, "Click to view/change machine properties.");

                //add to the panel space
                buildingsPanel.Controls.Add(tempPicBox);
                tempPicBox.BringToFront();
            }
        }

        

        private void buildingImage_Click(object sender, EventArgs e)
        {
            MachineOptions MO = new MachineOptions();
            MO.Show();


        }
    }



    /// <summary>
    /// Classes below
    /// </summary>

    public class ResourceRate
    {
        string name;
        double rate;

        public ResourceRate(string name, double rate)
        {
            this.name = name;
            this.rate = rate;
        }

        //accessors
        public string getName()
        {
            return name;
        }
        public double getRate()
        {
            return rate;
        }

        //setters
        public void setRate(double rate)
        {
            this.rate = rate;
        }

        public Image getImage()
        {
            Image itemImage;
            if (File.Exists("../../resources/splashes/" + name + ".png"))
            {
                itemImage = Image.FromFile("../../resources/splashes/" + name + ".png");
            }
            else
            {
                itemImage = Image.FromFile("../../resources/splashes/default_no_image.png");
            }
            Image resizedImage = new Bitmap(itemImage, new Size(75, 75));
            return resizedImage;
        }
    }

    //resources used to craft items (can be any item)
    public class resource
    {
        string name;
        int amount;
        //constructor
        public resource(string name, int amount)
        {
            this.name = name;
            this.amount = amount;
        }
        //accessors
        public int getAmount()
        {
            return amount;
        }
        public string getName()
        {
            return name;
        }
        //setter
        public void setAmount(int amount)
        {
            this.amount = amount;
        }
    }

    //items in the game
    public class item
    {
        string name;
        string craftedIn;       //machine used to make the item
        double baseProductionRate;  //how many per minute
        int producedPerProcess;           //how many items produced per feeder item
        List<resource> resourcesNeeded = new List<resource>();   //list of all resources down to the base unit used to make the item

        //accessors
        public List<resource> getResourcesNeeded()
        {
            return resourcesNeeded;
        }
        public double getBaseProductionRate()
        {
            return baseProductionRate;
        }
        public int getProducedPerProcess()
        {
            return producedPerProcess;
        }
        public string getCraftIn()
        {
            return craftedIn;
        }
        public string getName()
        {
            return name;
        }

        //constructor
        public item(string name)
        {
            this.name = name;
            //assign item properties
            switch (name){
                case "Not Specified":
                    craftedIn = "Not Specified";
                    baseProductionRate = 0;
                    producedPerProcess = 0;
                    break;
                case "Iron Ore":
                    craftedIn = "Miner MK1";
                    baseProductionRate = 30;
                    break;
                case "Copper Ore":
                    craftedIn = "Miner MK1";
                    baseProductionRate = 30;
                    break;
                case "Limestone":
                    craftedIn = "Miner MK1";
                    baseProductionRate = 30;
                    break;
                case "Coal":
                    craftedIn = "Miner MK1";
                    baseProductionRate = 30;
                    break;
                case "Iron Ingot":
                    craftedIn = "Smelter";
                    baseProductionRate = 30;
                    resourcesNeeded.Add(new resource("Iron Ore", 1));
                    producedPerProcess = 1;
                    break;
                case "Copper Ingot":
                    craftedIn = "Smelter";
                    baseProductionRate = 30;
                    resourcesNeeded.Add(new resource("Copper Ore", 1));
                    producedPerProcess = 1;
                    break;
                case "Iron Plate":
                    craftedIn = "Constructor";
                    baseProductionRate = 15;
                    resourcesNeeded.Add(new resource("Iron Ingot", 2));
                    resourcesNeeded.Add(new resource("Iron Ore",2));
                    producedPerProcess = 1;
                    break;
                case "Iron Rod":
                    craftedIn = "Constructor";
                    baseProductionRate = 15;
                    resourcesNeeded.Add(new resource("Iron Ingot", 1));
                    resourcesNeeded.Add(new resource("Iron Ore", 1));
                    producedPerProcess = 1;
                    break;
                case "Wire":
                    craftedIn = "Constructor";
                    baseProductionRate = 45;
                    resourcesNeeded.Add(new resource("Copper Ingot", 1));
                    resourcesNeeded.Add(new resource("Copper Ore", 1));
                    producedPerProcess = 3;
                    break;
                case "Cable":
                    craftedIn = "Constructor";
                    baseProductionRate = 15;
                    resourcesNeeded.Add(new resource("Wire", 2));
                    resourcesNeeded.Add(new resource("Copper Ingot", 2));
                    resourcesNeeded.Add(new resource("Copper Ore", 2));
                    producedPerProcess = 1;
                    break;
                case "Screws":
                    craftedIn = "Constructor";
                    baseProductionRate = 90;
                    resourcesNeeded.Add(new resource("Iron Ingot", 2));
                    resourcesNeeded.Add(new resource("Iron Ore", 2));
                    producedPerProcess = 12;
                    break;
                case "Screws (from rods)":
                    craftedIn = "Constructor";
                    baseProductionRate = 90;
                    resourcesNeeded.Add(new resource("Iron Rod", 1));
                    resourcesNeeded.Add(new resource("Iron Ingot", 1));
                    resourcesNeeded.Add(new resource("Iron Ore", 1));
                    producedPerProcess = 6;
                    break;
                case "Concrete":
                    craftedIn = "Constructor";
                    baseProductionRate = 15;
                    resourcesNeeded.Add(new resource("Limestone", 3));
                    producedPerProcess = 1;
                    break;
                case "Biomass (Leaves)":
                    craftedIn = "Constructor";
                    baseProductionRate = 90;
                    resourcesNeeded.Add(new resource("Leaves", 10));
                    producedPerProcess = 6;
                    break;
                case "Biomass (Wood)":
                    craftedIn = "Constructor";
                    baseProductionRate = 375;
                    resourcesNeeded.Add(new resource("Wood", 5));
                    producedPerProcess = 25;
                    break;
                case "Biomass (Mycelia)":
                    craftedIn = "Constructor";
                    baseProductionRate = 150;
                    resourcesNeeded.Add(new resource("Mycelia", 10));
                    producedPerProcess = 10;
                    break;
                case "Biomass (Alien Carapace)":
                    craftedIn = "Constructor";
                    baseProductionRate = 1500;
                    resourcesNeeded.Add(new resource("Alien Carapace", 1));
                    producedPerProcess = 100;
                    break;
                case "Biofuel":
                    craftedIn = "Constructor";
                    baseProductionRate = 30;
                    resourcesNeeded.Add(new resource("Biomass", 4));
                    producedPerProcess = 2;
                    break;
                case "Reinforced Iron Plate":
                    craftedIn = "Assembler";
                    baseProductionRate = 5;
                    resourcesNeeded.Add(new resource("Iron Plate", 4));
                    resourcesNeeded.Add(new resource("Screws", 24));
                    resourcesNeeded.Add(new resource("Iron Ingot", 12));
                    resourcesNeeded.Add(new resource("Iron Ore", 12));
                    producedPerProcess = 1;
                    break;
                case "Reinforced Iron Plate (Hard Drive 1)":
                    craftedIn = "Assembler";
                    baseProductionRate = 7.5;
                    resourcesNeeded.Add(new resource("Iron Plate", 10));
                    resourcesNeeded.Add(new resource("Screws", 24));
                    resourcesNeeded.Add(new resource("Iron Ingot", 24));
                    resourcesNeeded.Add(new resource("Iron Ore", 24));
                    producedPerProcess = 3;
                    break;
                case "Rotor":
                    craftedIn = "Assembler";
                    baseProductionRate = 6;
                    resourcesNeeded.Add(new resource("Iron Rod", 3));
                    resourcesNeeded.Add(new resource("Screws", 22));
                    resourcesNeeded.Add(new resource("Iron Ingot", 7));
                    resourcesNeeded.Add(new resource("Iron Ore", 7));
                    //leftover 2 screws
                    producedPerProcess = 1;
                    break;
                case "Modular Frame":
                    craftedIn = "Assembler";
                    baseProductionRate = 4;
                    resourcesNeeded.Add(new resource("Reinforced Iron Plate", 3));
                    resourcesNeeded.Add(new resource("Iron Rod", 6));
                    resourcesNeeded.Add(new resource("Screws", 72));
                    resourcesNeeded.Add(new resource("Iron Ingot", 38));
                    resourcesNeeded.Add(new resource("Iron Ore", 38));
                    producedPerProcess = 1;
                    break;
                case "Fabric":
                    craftedIn = "Assembler";
                    baseProductionRate = 15;
                    resourcesNeeded.Add(new resource("Mycelia", 1));
                    resourcesNeeded.Add(new resource("Leaves", 2));
                    producedPerProcess = 1;
                    break;

                default:
                    craftedIn = "Item not found";
                    baseProductionRate = 0;
                    producedPerProcess = 0;
                    break;
            }
        }
    }

    //production buildings in the game
    public class building
    {
        string name;
        double powerUsage;  //in MW
        string producing;
        //constructor
        public building(string name)
        {
            this.name = name;
            switch (name)
            {

                case "Miner MK1":
                    powerUsage = 5;
                    break;
                case "Smelter":
                    powerUsage = 4;
                    break;
                case "Constructor":
                    powerUsage = 4;
                    break;
                case "Assembler":
                    powerUsage = 15;
                    break;

                default:
                    powerUsage = 0;
                    break;
            }
        }
        //accessors
        public string getName()
        {
            return name;
        }
        public double getPowerUsage()
        {
            return powerUsage;
        }

        //setters
        public void setProducing(string producing)
        {
            this.producing = producing;
        }

        public void applyClock(double clock)
        {
            powerUsage = 4 * Math.Pow(clock, 1.636);
        }
        
    }

    public class buildingCount
    {
        //used for storing a building count
        string name;
        int amount;

        //constructor
        public buildingCount(string name)
        {
            this.name = name;
            amount = 1;
        }

        //accessors
        public string getName()
        {
            return name;
        }
        public int getAmount()
        {
            return amount;
        }


        //setters
        public void setName(string name)
        {
            this.name = name;
        }
        public void setAmount(int amount)
        {
            this.amount = amount;
        }

        public Image getImage()
        {
            Image buildingImage;
            if (File.Exists("../../resources/splashes/" + name + ".png"))
            {
                buildingImage = Image.FromFile("../../resources/splashes/" + name + ".png");
            }
            else
            {
                buildingImage = Image.FromFile("../../resources/splashes/default_no_image.png");
            }
            Image resizedImage = new Bitmap(buildingImage, new Size(75, 75));
            return resizedImage;
        }
    }

    //power generation buildings in the game
    public class generator
    {
        string name;
        double powerGen;
        double productionRate;  //used for clocking
        //constructor
        public generator(string name)
        {
            this.name = name;
            if(name.Equals("Biomass Burner"))
            {
                powerGen = 20;
                productionRate = 1;
            }
            else if(name.Equals("Coal Generator"))
            {
                powerGen = 50;
                productionRate = 1;
            }
            else
            {
                powerGen = 0;
                productionRate = 0;
            }
        }

        //accessors
        public string getName()
        {
            return name;
        }
        public double getPowerGen()
        {
            return powerGen;
        }

        public Image getImage()
        {
            Image generatorImage;
            if (File.Exists("../../resources/splashes/" + name + ".png"))
            {
                generatorImage = Image.FromFile("../../resources/splashes/" + name + ".png");
            }
            else
            {
                generatorImage = Image.FromFile("../../resources/splashes/default_no_image.png");
            }
            Image resizedImage = new Bitmap(generatorImage, new Size(75, 75));
            return resizedImage;
        }

        public void applyClock(double clock)
        {
            if(name.Equals("Coal Generator"))
                powerGen = 50 * Math.Pow(clock, .7693);
            else if(name.Equals("Biomass Burner"))
                powerGen = 20 * Math.Pow(clock, .7693);
        }
    }

}


