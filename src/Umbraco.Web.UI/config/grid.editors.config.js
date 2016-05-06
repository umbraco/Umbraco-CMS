[
	{
	    "name": "Rich text editor",
	    "alias": "rte",
	    "view": "rte",
	    "icon": "icon-article"
	},
	{
	    "name": "Image",
	    "alias": "media",
	    "view": "media",
	    "icon": "icon-picture"
	},
	{
	    "name": "Image wide",
	    "alias": "media_wide",
	    "view": "media",
	    "render": "/App_Plugins/Grid/Editors/Render/media_wide.cshtml",
	    "icon": "icon-picture"
	},
	{
	    "name": "Image wide cropped",
	    "alias": "media_wide_cropped",
	    "view": "media",
	    "render": "media",
	    "icon": "icon-picture",
	    "config": {
	    	"size": {
	    		"width": 1920,
	    		"height": 700
	    	}
	    }
	},
	{
	    "name": "Image rounded",
	    "alias": "media_round",
	    "view": "media",
	    "render": "/App_Plugins/Grid/Editors/Render/media_round.cshtml",
	    "icon": "icon-picture"
	},
	{
	    "name": "Image w/ text right",
	    "alias": "media_text_right",
	    "view": "/App_Plugins/Grid/Editors/Views/media_with_description.html",
	    "render": "/App_Plugins/Grid/Editors/Render/media_text_right.cshtml",
	    "icon": "icon-picture"
	},
	{
	    "name": "Macro",
	    "alias": "macro",
	    "view": "macro",
	    "icon": "icon-settings-alt"
	},
	{
	    "name": "Embed",
	    "alias": "embed",
	    "view": "embed",
	    "render": "/App_Plugins/Grid/Editors/Render/embed_videowrapper.cshtml",
	    "icon": "icon-movie-alt"
	},
	{
        "name": "Banner Headline",
        "alias": "banner_headline",
        "view": "textstring",
        "icon": "icon-coin",
        "config": {
            "style": "font-size: 36px; line-height: 45px; font-weight: bold; text-align:center",
            "markup": "<h1 style='font-size:62px;text-align:center'>#value#</h1>"
        }
    },
    {
        "name": "Banner Tagline",
        "alias": "banner_tagline",
        "view": "textstring",
        "icon": "icon-coin",
        "config": {
            "style": "font-size: 25px; line-height: 35px; font-weight: normal; text-align:center",
            "markup": "<h2 style='font-weight: 100; font-size: 40px;text-align:center'>#value#</h2>"
        }
    },
    {
        "name": "Headline",
        "alias": "headline",
        "view": "textstring",
        "icon": "icon-coin",
        "config": {
            "style": "font-size: 36px; line-height: 45px; font-weight: bold",
            "markup": "<h1>#value#</h1>"
        }
    },
    {
        "name": "Headline centered",
        "alias": "headline_centered",
        "view": "textstring",
        "icon": "icon-coin",
        "config": {
            "style": "font-size: 30px; line-height: 45px; font-weight: bold; text-align:center;",
            "markup": "<h1 style='text-align:center;'>#value#</h1>"
        }
    },
    {
        "name": "Abstract",
        "alias": "abstract",
        "view": "textstring",
        "icon": "icon-coin",
        "config": {
            "style": "font-size: 16px; line-height: 20px; font-weight: bold;",
            "markup": "<h3>#value#</h3>"
        }
    },
    {
        "name": "Paragraph",
        "alias": "paragraph",
        "view": "textstring",
        "icon": "icon-font",
        "config": {
            "style": "font-size: 16px; line-height: 20px; font-weight: light;",
            "markup": "<p>#value#</p>"
        }
    },
	{
	    "name": "Quote",
	    "alias": "quote",
	    "view": "textstring",
	    "icon": "icon-quote",
	    "config": {
	        "style": "border-left: 3px solid #ccc; padding: 10px; color: #ccc; font-family: serif; font-variant: italic; font-size: 18px",
	        "markup": "<blockquote>#value#</blockquote>"
	    }
	},
	{
	    "name": "Quote with description",
	    "alias": "quote_D",
	    "view": "/App_Plugins/Grid/Editors/Views/quote_with_description.html",
	    "render": "/App_Plugins/Grid/Editors/Render/quote_with_description.cshtml",
	    "icon": "icon-quote",
	    "config": {
	        "style": "border-left: 3px solid #ccc; padding: 10px; color: #ccc; font-family: serif; font-variant: italic; font-size: 18px"
	    }
	},
	{
	    "name": "Code",
	    "alias": "code",
	    "view": "textstring",
	    "icon": "icon-code",
	    "config": {
	        "style": "overflow: auto;padding: 6px 10px;border: 1px solid #ddd;border-radius: 3px;background-color: #f8f8f8;font-size: .9rem;font-family: 'Courier 10 Pitch', Courier, monospace;line-height: 19px;",
	        "markup": "<pre>#value#</pre>"
	    }
	}
]