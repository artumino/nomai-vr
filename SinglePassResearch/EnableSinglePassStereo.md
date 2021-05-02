This is a research branch to find out if it's possible to enable singlepass stereo. By disabling every image effect it's still not possible to have a non warped stereo image. One of the last shaders seems to shift the framebuffer and breaks everything.

## Get Single Pass Stereo to work
1) Patch globalmanagers with patched version of UnityPatcher (singlepass_stereo_patcher.patch)
2) Copy from a project built with Single Pass Stereo enabled the files under "{Game}_Data/Resources", this replaces non stereo default shaders with properly built ones.
3) Disable every image effect
4) ???
