'use strict';

module.exports = {
    sources: {

        //less files used by backoffice and preview
        //processed in the less task
        less: {
            installer: { files: ["./src/less/installer.less"], out: "installer.css" },
            nonodes: { files: ["./src/less/pages/nonodes.less"], out: "nonodes.style.min.css"},
            preview: { files: ["./src/less/canvas-designer.less"], out: "canvasdesigner.css" },
            umbraco: { files: ["./src/less/belle.less"], out: "umbraco.css" }
        },

        //js files for backoffie
        //processed in the js task
        js: {
            preview: { files: ["./src/preview/**/*.js"], out: "umbraco.preview.js" },
            installer: { files: ["./src/installer/**/*.js"], out: "umbraco.installer.js" },
            controllers: { files: ["./src/{views,controllers}/**/*.controller.js"], out: "umbraco.controllers.js" },
            directives: { files: ["./src/common/directives/**/*.js"], out: "umbraco.directives.js" },
            filters: { files: ["./src/common/filters/**/*.js"], out: "umbraco.filters.js" },
            resources: { files: ["./src/common/resources/**/*.js"], out: "umbraco.resources.js" },
            services: { files: ["./src/common/services/**/*.js"], out: "umbraco.services.js" },
            security: { files: ["./src/common/interceptors/**/*.js"], out: "umbraco.interceptors.js" }
        },

        //selectors for copying all views into the build
        //processed in the views task
        views:{
            umbraco: {files: ["./src/views/**/*.html"], folder: ""},
            installer: {files: ["./src/installer/steps/*.html"], folder: "install/"}
        },

        //globs for file-watching
        globs:{
            views: "./src/views/**/*.html",
            less: "./src/less/**/*.less",
            js: "./src/*.js",
            lib: "./lib/**/*",
            assets: "./src/assets/**"
        }
    },
    root: "../Umbraco.Web.UI/",
    targets: {
        js: "Umbraco/js/",
        lib: "Umbraco/lib/",
        views: "Umbraco/views/",
        css: "Umbraco/assets/css/",
        assets: "Umbraco/assets/"
    }
};
