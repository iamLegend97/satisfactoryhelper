using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Satisfactory_Helper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void calcButton_Click(object sender, EventArgs e)
        {
            //initialize list of values and set equal to user input
            List<Double> amountPerMin = new List<Double>();
            List<string> textBoxes = new List<string> {
                textBox1.Text,
                textBox2.Text,
                textBox3.Text,
                textBox4.Text,
                textBox5.Text,
                textBox6.Text,
                textBox7.Text,
                textBox8.Text,
                textBox9.Text,
                textBox10.Text
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
            //List<building> buildingsNeeded = new List<building>();
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
            
            //clear all the labels and images and set new values
            List<Label> itemsNeededLabels = new List<Label>
            {
                //items
                label5,
                label6,
                label7,
                label8,
                label9,
                label10,
                label11,
                label12,
                label13,
                label14,
                label15,
                label16,

                //buildings
                label17,
                label18,
                label19,
                label20,
            };
            List<PictureBox> itemsNeededImages = new List<PictureBox>
            {
                //items
                pictureBox1,
                pictureBox2,
                pictureBox3,
                pictureBox4,
                pictureBox5,
                pictureBox6,
                pictureBox7,
                pictureBox8,
                pictureBox9,
                pictureBox10,
                pictureBox11,
                pictureBox12,

                //buildings
                pictureBox13,
                pictureBox14,
                pictureBox15,
                pictureBox16
            };
            
            for(int i = 0; i < itemsNeededLabels.Count; i++)
            {
                //clearing
                itemsNeededLabels.ElementAt(i).Text = "";
                itemsNeededImages.ElementAt(i).Image = null;
            }
            for(int i = 0; i < totalResourcePerMin.Count; i++)
            {
                //new values
                itemsNeededLabels.ElementAt(i).Text = Convert.ToString(totalResourcePerMin.ElementAt(i).getRate()) + " " + totalResourcePerMin.ElementAt(i).getName() + " /min";
                itemsNeededImages.ElementAt(i).Image = totalResourcePerMin.ElementAt(i).getImage();
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
                //not a base unit so we can access all of the data
                int numMachinesNeeded = (int)(totalResourcePerMin.ElementAt(i).getRate() / Item.getBaseProductionRate());
                string machineName = Item.getCraftIn();
                double lastMachineRate = 0;
                double lastMachineUnderclock = 0;
                bool uneven = false;
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
                    if(j == numMachinesNeeded - 1 && uneven)
                    {
                        //we are at the last item so set the clock rate
                        buildings.ElementAt(j).applyClock(lastMachineUnderclock);
                    }
                }
            }
            for(int i = 0; i < buildings.Count; i++)
            {
                //calculate total power consumption from all machines
                totalPowerConsumption += buildings.ElementAt(i).getPowerUsage();
            }
            label22.Text = Convert.ToString(totalPowerConsumption) + " MW";

            //get total amount of all machines
            List<int> totals = new List<int>
            {
                0,
                0,
                0,
                0
            };
            for (int j = 0; j < buildings.Count; j++)
            {
                string name = buildings.ElementAt(j).getName();
                switch (name)
                {
                    case "Miner MK1":
                        totals[0] += 1;
                        break;
                    case "Smelter":
                        totals[1] += 1;
                        break;
                    case "Constructor":
                        totals[2] += 1;
                        break;
                    case "Assembler":
                        totals[3] += 1;
                        break;
                    default:
                        break;
                }
            }

            List<string> buildingsList = new List<string>
            {
                "Miner MK1",
                "Smelter",
                "Constructor",
                "Assembler"
            };
            for (int i = 12; i < itemsNeededImages.Count; i++)
            {
                int j = i - 12;
                //display images and amounts for all machines
                itemsNeededLabels.ElementAt(i).Text = Convert.ToString(totals.ElementAt(j)) + " " + buildingsList.ElementAt(j);
                itemsNeededImages.ElementAt(i).Image = new building(buildingsList.ElementAt(j)).getImage();

            }
            

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
            Image itemImage = Image.FromFile("../../resources/" + name + ".png");
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
                case "Screws (from ingots)":
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
        double productionRate;
        //constructor
        public building(string name)
        {
            this.name = name;
            switch (name)
            {

                case "Miner MK1":
                    powerUsage = 5;
                    productionRate = 1;
                    break;
                case "Smelter":
                    powerUsage = 4;
                    productionRate = 1;
                    break;
                case "Constructor":
                    powerUsage = 4;
                    productionRate = 1;
                    break;
                case "Assembler":
                    powerUsage = 15;
                    productionRate = 1;
                    break;

                default:
                    powerUsage = 0;
                    productionRate = 0;
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

        public Image getImage()
        {
            Image buildingImage = Image.FromFile("../../resources/" + name + ".png");
            Image resizedImage = new Bitmap(buildingImage, new Size(75, 75));
            return resizedImage;
        }

        public void applyClock(double clock)
        {
            powerUsage = 4 * Math.Pow(clock, 1.636);
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
            Image generatorImage = Image.FromFile("../../resources/" + name + ".png");
            Image resizedImage = new Bitmap(generatorImage, new Size(75, 75));
            return resizedImage;
        }

        public void applyClock(double clock)
        {
            if(name.Equals("Coal Generator"))
                powerGen = 50 * Math.Pow(clock, .7693);
        }
    }

}


