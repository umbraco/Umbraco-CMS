/*-----------------------------------------------------------------------
* Sa11y: the accessibility quality assurance assistant.
* Bookmarklet: English
* @author: Development led by Adam Chaboryk, CPWA
* @acknowledgements: https://sa11y.netlify.app/acknowledgements/
* @license: https://github.com/ryersondmp/sa11y/blob/master/LICENSE.md
* Copyright (c) 2020 - 2022 Toronto Metropolitan University
* The above copyright notice shall be included in all copies or substantial portions of the Software.
------------------------------------------------------------------------*/

const version = "2.3.5"; //Version

if (typeof Sa11y !== "object") {
    /* Append sa11y.css */
    const sa11ycss = document.createElement("link");
    sa11ycss.setAttribute("rel", "stylesheet");
    sa11ycss.setAttribute("href", `https://cdn.jsdelivr.net/gh/ryersondmp/sa11y@${version}/dist/css/sa11y.min.css`);
    sa11ycss.setAttribute("type", "text/css");

    const bodyheader = document.getElementsByTagName("head")[0];
    bodyheader.appendChild(sa11ycss);

    const combine = document.createElement("script");
    combine.src = `https://cdn.jsdelivr.net/combine/gh/ryersondmp/sa11y@${version}/dist/js/lang/en.umd.js,gh/ryersondmp/sa11y@${version}/dist/js/sa11y.umd.min.js`;

    document.body.appendChild(combine);
    combine.onload = combine.onreadystatechange = function() {
        Sa11y.Lang.addI18n(Sa11yLangEn.strings);
        new Sa11y.Sa11y({
            detectSPArouting: true,
        });
    };
} else {
    alert('Drag the "Sa11y" button into your bookmarks bar. Then click the bookmark on any webpage.');
}