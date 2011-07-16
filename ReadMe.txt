This continues from  step 6 in the Fuel Cell example "FuelCell: "Ships" Passing in the Night"
http://msdn.microsoft.com/en-us/library/dd254739.aspx

Whats added is support for WP7.  The Fuel Cell sample code linked above was built for XNA 4, but not WP7.
So, we needed some input from the player, and, thankfully MS already had built some WP7 Thumbsticks
http://create.msdn.com/en-US/sample/touchthumbsticks


With the integrated thumbsticks you can now control your Fuel Carrier using XNA's Touch Collection.
Notice a touch on the left side is for steering, a touch on the right could be used for FPS camera view or shooting/aiming in 3rd person.

This codebase is by no means a good example of how to write clean and fast running code, it's mearly a starter project for doing something in 3D.

I did however tidy things up slightly from the MS Fuel Cell project (I actually built it from following steps vs. using the finished project)
I advise you to add your own polish using the last step MS provides and or a framework for screens/state that you may have already built.
You should be able to continue from this project to that step for the most part taking into consideration some of the changes I've made to how things are structured in /GameObjects



Why did I do this?  
  1. I have three 2D games already at CheeseZombieGames.com and wanted to start learning some 3D.  
  2. I didnt find the 3D Phone samples in the education section of AppHub all that entertaining.
  3. I've used a ton of code from MS in the past on my projects and I thought it was about time to give something back.


Enjoy, do with this what you want.  I hope to add Frustrum culling here shortly.
Marc