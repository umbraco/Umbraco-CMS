Umbraco Cms Clear
--
----

Once the solution has been used to run a site, one may want to "reset" the solution in order to run a fresh new site again.

## Fast

At the very minimum, you want

    git clean -Xdf src/Umbraco.Web.UI/App_Data
    rm src/Umbraco.Web.UI/web.config

Then, a simple 'Rebuild All' in Visual Studio will recreate a fresh `web.config` but should be quite fast (since it does not really need to rebuild anything).

The `clean` Git command force (`-f`) removes (`-X`, note the capital X) all files and directories (`-d`) that are ignored by Git.

This will leave medias and views around, but in most cases, it will be enough.

## More

To perform a more complete clear, you will want to also delete the content of the media, views, masterpages, scripts... directories.

## Full

The following command will force remove all untracked files and directories, be they ignored by Git or not. Combined with `git reset` it can recreate a pristine working directory. 

    git clean -xdf .
    
## Docs

See
* git [clean](<https://git-scm.com/docs/git-clean>)
* git [reset](<https://git-scm.com/docs/git-reset>)