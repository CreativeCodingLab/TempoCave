# TempoCave
Visualizing dynamic connectome datasets to support cognitive behavioral therapy (CBT)
![](/READMEImages/teaser.PNG)
![](/READMEImages/Fig2_new.png)
![](/READMEImages/Fig3-short.png)
![](/READMEImages/Fig4-timeseries.png)
TempoCave is a novel visualization application for analyzing dynamic brain networks, or connectomes. Tempocave provides a range of functionality to explore metrics related to the activity patterns and modular affiliations of different regions in the brain. These patterns are calculated by processing raw data retrieved functional magnetic resonance imaging (fMRI) scans, which creates a network of weighted edges between each brain region, where the weight indicates how likely these regions are to activate  synchronously. In particular, we support the analysis needs of clinical psychologists, who examine these modular affiliations and weighted edges and their temporal dynamics, utilizing them to understand relationships between neurological disorders and brain activity, which could have significant impact on the way in which patients are diagnosed and treated. TempoCave supports a range of comparative tasks and runs both in a desktop mode and in an immersive mode. 
# TempoCave article
An article introducing TempoCave can be found in arxiv — [TempoCave: Visualizing Dynamic Connectome Datasets to Support Cognitive Behavioral Therapy](https://arxiv.org/abs/1906.07837)
# How to use TempoCave
You can download the executible version [here](https://drive.google.com/a/ucsc.edu/file/d/1Is6XfGsFR2iLBrW2W_NVurXw-cHLl--I/view?usp=sharing) to try it out, which does not require Unity platform. 
### Instructions 
Click TempoCave.exe to launch the app 

Frist Scene Controls: 
- Mouse scroll or left arrow/right arrow on keyboard to browse the connectomes available 
- Mouse left click to select one or two connectomes 
- Mouse left click/press enter on keyboard to inspect the connectomes selected 

Second Scene Controls: 
- Hold mouse left button to rotate the connectome 
- Press mouse scroll/right button to rotate the whole scene 
- Click on the node to select/deselect the edges 
- Press 0 on keyboard to focus on two connectomes 
- Press 1 on keyboard to focus on first connectome 
- Press 2 on keyboard to focus on second connectome 
- Scroll up on mouse (W on keyboard) to zoom in 
- Scroll down on mouse (S on keyboard) to zoom out 
- Press A (D) on keyboard to move the scene to left (Right) 
- Press compare to overlay the two connectomes 

### Upload Custom Data 
- You can upload your custom data folder under the connectome data pathway: TempoCave/Assets/Resources/Data 
- Folder structure of connectome data is shown in images below: left image is for static connectome data and right image is for dynamic connectome data 
- Folder names and file names are case sensitive 
- Note that each csv file need to have a header 
- Edge connectivity data and topology data should be normalized to (-1, 1) (Both positive and negative connectivities can be shown on TempoCave)

![](/READMEImages/FolderInstruction.png) 
