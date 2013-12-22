osu! Square Jump Map Maker
===================

I wanted to map many songs throughout the years I've played osu!; however I failed in doing so due to the amount of time it took and my design creativity level (very low). At the same time there were no maps available for the songs from other mappers.

This is my attempt at trying to generate maps for my favourite songs which are somewhat challenging to play.

Okay, so... How do I use it?
----------------------------

A beatmap requires two things to be accepted by this program - a start control point and a finish control point. For example, this is how your Timing Setup Panel in the beatmap editor "timing" tab should look:

![](http://u.smgi.me/avm.png "Timing Setup Panel")

It is not required that a beatmap must have no hitobjects, but the application will not stop where there is a hitobject, so you may end up with double circles on beats if other hitobjects are present before the application is run.

Shameless Plugs
---------------

This application uses one of my other projects, the [osu! Beatmap API](https://github.com/smoogipooo/osu-Beatmap-API). This project was also created partly as a demonstration of the osu! Beatmap API. If you're interested in working with beatmaps outside of osu! (i.e. without relying on AIMod), please check out the aforementioned API.

Thank you :)