'use strict';

module.exports = {
    compile: {
        build: {
            sourcemaps: false,
            embedtemplates: true,
            minify: true
        },
        dev: {
            sourcemaps: true,
            embedtemplates: true,
            minify: false
        },
        test: {
            sourcemaps: false,
            embedtemplates: true,
            minify: true
        }
    },
    sources: {

        // less files used by backoffice and preview
        // processed in the less task
        less: {
            installer: { files: "./src/less/installer.less", watch: "./src/less/**/*.less", out: "installer.min.css" },
            nonodes: { files: "./src/less/pages/nonodes.less", watch: "./src/less/**/*.less", out: "nonodes.style.min.css"},
            preview: { files: "./src/less/canvas-designer.less", watch: "./src/less/**/*.less", out: "canvasdesigner.min.css" },
            umbraco: { files: "./src/less/belle.less", watch: "./src/**/*.less", out: "umbraco.min.css" },
            rteContent: { files: "./src/less/rte-content.less", watch: "./src/less/**/*.less", out: "rte-content.css" }
        },

        // js files for backoffice
        // processed in the js task
        js: {
            websitepreview: { files: "./src/websitepreview/**/*.js", out: "umbraco.websitepreview.js" },
            preview: { files: "./src/preview/**/*.js", out: "umbraco.preview.js" },
            installer: { files: "./src/installer/**/*.js", out: "umbraco.installer.js" },
            filters: { files: "./src/common/filters/**/*.js", out: "umbraco.filters.js" },
            resources: { files: "./src/common/resources/**/*.js", out: "umbraco.resources.js" },
            services: { files: ["./src/common/services/**/*.js", "./src/utilities.js"], out: "umbraco.services.js" },
            security: { files: "./src/common/interceptors/**/*.js", out: "umbraco.interceptors.js" },

            //the controllers for views
            controllers: {
                files: [
                    "./src/views/**/*.controller.js",
                    "./src/*.controller.js"
                ], out: "umbraco.controllers.js"
            },

            //directives/components
            // - any JS file found in common / directives or common/ components
            // - any JS file found inside views that has the suffix .directive.js or .component.js
            directives: {
                files: [
                    "./src/common/directives/_module.js",
                    "./src/{common/directives,common/components}/**/*.js",
                    "./src/views/**/*.{directive,component}.js"
                ],
                out: "umbraco.directives.js"
            }

        },

        //selectors for copying all views into the build
        //processed in the views task
        views:{
            views: {files: "./src/views/**/*.html", folder: ""},
            directives: {files: "./src/common/directives/**/*.html", folder: ""},
            components: {files: "./src/common/components/**/*.html", folder: ""},
            installer: {files: "./src/installer/steps/*.html", folder: "install/"}
        },

        //globs for file-watching
        globs:{
            views: ["./src/views/**/*.html", "./src/common/directives/**/*.html", "./src/common/components/**/*.html" ],
            less: "./src/less/**/*.less",
            js: "./src/*.js",
            lib: "./lib/**/*",
            assets: "./src/assets/**"
        }
    },
    roots: ["../Umbraco.Cms.StaticAssets/wwwroot/"],
    targets: {
        js: "umbraco/js/",
        lib: "umbraco/lib/",
        views: "umbraco/views/",
        css: "umbraco/assets/css/",
        assets: "umbraco/assets/"
    }
};
