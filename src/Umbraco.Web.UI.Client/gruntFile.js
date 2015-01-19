module.exports = function (grunt) {

    

    // Default task.
    grunt.registerTask('default', ['jshint:dev', 'build', 'karma:unit']);
    grunt.registerTask('dev', ['jshint:dev', 'build-dev', 'webserver', 'open:dev', 'watch']);
    grunt.registerTask('docserve', ['docs:api', 'connect:docserver', 'open:docs', 'watch:docs']);
    grunt.registerTask('vs', ['jshint:dev', 'build-dev', 'watch']);

    //TODO: Too much watching, this brings windows to it's knees when in dev mode
    //run by the watch task
    grunt.registerTask('watch-js', ['jshint:dev', 'concat', 'copy:app', 'copy:mocks', 'copy:canvasdesigner', 'copy:vs', 'karma:unit']);
    grunt.registerTask('watch-less', ['recess:build', 'recess:installer', 'recess:canvasdesigner', 'copy:canvasdesigner', 'copy:assets', 'copy:vs']);
    grunt.registerTask('watch-html', ['copy:views', 'copy:vs']);
    grunt.registerTask('watch-installer', ['concat:install', 'concat:installJs', 'copy:installer', 'copy:vs']);
    grunt.registerTask('watch-canvasdesigner', ['copy:canvasdesigner', 'concat:canvasdesignerJs', 'copy:vs']);
    grunt.registerTask('watch-test', ['jshint:dev', 'karma:unit']);

    //triggered from grunt dev or grunt
    grunt.registerTask('build', ['clean', 'concat', 'recess:min', 'recess:installer', 'recess:canvasdesigner', 'bower', 'copy']);

    //build-dev doesn't min - we are trying to speed this up and we don't want minified stuff when we are in dev mode
    grunt.registerTask('build-dev', ['clean', 'concat', 'recess:build', 'recess:installer', 'bower', 'copy']);

    //utillity tasks
    grunt.registerTask('docs', ['ngdocs']);
    grunt.registerTask('webserver', ['connect:devserver']);


    // Print a timestamp (useful for when watching)
    grunt.registerTask('timestamp', function () {
        grunt.log.subhead(Date());
    });

    // Project configuration.
    grunt.initConfig({
        buildVersion: grunt.option('buildversion') || '7',
        connect: {
            devserver: {
                options: {
                    port: 9990,
                    hostname: '0.0.0.0',
                    base: './build',
                    middleware: function (connect, options) {
                        return [
                          //uncomment to enable CSP
                          // util.csp(),
                          //util.rewrite(),
                          connect.favicon('images/favicon.ico'),
                          connect.static(options.base),
                          connect.directory(options.base)
                        ];
                    }
                }
            },
            testserver: {},
            docserver: {
                options: {
                    port: 8880,
                    hostname: '0.0.0.0',
                    base: './docs/api',
                    middleware: function (connect, options) {
                        return [
                          //uncomment to enable CSP
                          // util.csp(),
                          //util.rewrite(),
                          connect.static(options.base),
                          connect.directory(options.base)
                        ];
                    }
                }
            },
        },

        open: {
            dev: {
                path: 'http://localhost:9990/belle/'
            },
            docs: {
                path: 'http://localhost:8880/index.html'
            }
        },

        distdir: 'build/belle',
        vsdir: '../Umbraco.Web.UI/umbraco',
        pkg: grunt.file.readJSON('package.json'),
        banner:
        '/*! <%= pkg.title || pkg.name %>\n' +
        '<%= pkg.homepage ? " * " + pkg.homepage + "\\n" : "" %>' +
        ' * Copyright (c) <%= grunt.template.today("yyyy") %> <%= pkg.author %>;\n' +
        ' * Licensed <%= _.pluck(pkg.licenses, "type").join(", ") %>\n */\n',
        src: {
            js: ['src/**/*.js', 'src/*.js'],
            common: ['src/common/**/*.js'],
            specs: ['test/**/*.spec.js'],
            scenarios: ['test/**/*.scenario.js'],
            samples: ['sample files/*.js'],
            html: ['src/index.html', 'src/install.html'],

            everything: ['src/**/*.*', 'test/**/*.*', 'docs/**/*.*'],

            tpl: {
                app: ['src/views/**/*.html'],
                common: ['src/common/**/*.tpl.html']
            },
            less: ['src/less/belle.less'], // recess:build doesn't accept ** in its file patterns
            prod: ['<%= distdir %>/js/*.js']
        },

        clean: ['<%= distdir %>/*'],

        copy: {
            assets: {
                files: [{ dest: '<%= distdir %>/assets', src: '**', expand: true, cwd: 'src/assets/' }]
            },

            config: {
                files: [{ dest: '<%= distdir %>/../config', src: '**', expand: true, cwd: 'src/config/' }]
            },

            installer: {
                files: [{ dest: '<%= distdir %>/views/install', src: '**/*.html', expand: true, cwd: 'src/installer/steps' }]
            },

            canvasdesigner: {
                files: [
                    { dest: '<%= distdir %>/preview', src: '**/*.html', expand: true, cwd: 'src/canvasdesigner' },
                    { dest: '<%= distdir %>/preview/editors', src: '**/*.html', expand: true, cwd: 'src/canvasdesigner/editors' },
                    { dest: '<%= distdir %>/assets/less', src: '**/*.less', expand: true, cwd: 'src/canvasdesigner/editors' },
                    { dest: '<%= distdir %>/js', src: 'canvasdesigner.config.js', expand: true, cwd: 'src/canvasdesigner/config' },
                    { dest: '<%= distdir %>/js', src: 'canvasdesigner.palettes.js', expand: true, cwd: 'src/canvasdesigner/config' },
                    { dest: '<%= distdir %>/js', src: 'canvasdesigner.front.js', expand: true, cwd: 'src/canvasdesigner' }
                ]
            },

            vendor: {
                files: [{ dest: '<%= distdir %>/lib', src: '**', expand: true, cwd: 'lib/' }]
            },
            views: {
                files: [{ dest: '<%= distdir %>/views', src: ['**/*.*', '!**/*.controller.js'], expand: true, cwd: 'src/views/' }]
            },
            app: {
                files: [
                    { dest: '<%= distdir %>/js', src: '*.js', expand: true, cwd: 'src/' }
                ]
            },
            mocks: {
                files: [{ dest: '<%= distdir %>/js', src: '*.js', expand: true, cwd: 'src/common/mocks/' }]
            },
            vs: {
                files: [
                    //everything except the index.html root file!
                    //then we need to figure out how to not copy all the test stuff either!?
                    { dest: '<%= vsdir %>/assets', src: '**', expand: true, cwd: '<%= distdir %>/assets' },
                    { dest: '<%= vsdir %>/js', src: '**', expand: true, cwd: '<%= distdir %>/js' },                    
                    { dest: '<%= vsdir %>/views', src: '**', expand: true, cwd: '<%= distdir %>/views' },
                    { dest: '<%= vsdir %>/preview', src: '**', expand: true, cwd: '<%= distdir %>/preview' },
                    { dest: '<%= vsdir %>/lib', src: '**', expand: true, cwd: '<%= distdir %>/lib' }
                ]
            }
        },

        karma: {
            unit: { configFile: 'test/config/karma.conf.js', keepalive: true },
            e2e: { configFile: 'test/config/e2e.js', keepalive: true },
            watch: { configFile: 'test/config/unit.js', singleRun: false, autoWatch: true, keepalive: true }
        },

        concat: {
            index: {
                src: ['src/index.html'],
                dest: '<%= distdir %>/index.html',
                options: {
                    process: true
                }
            },
            install: {
                src: ['src/installer/installer.html'],
                dest: '<%= distdir %>/installer.html',
                options: {
                    process: true
                }
            },

            installJs: {
                src: ['src/installer/**/*.js'],
                dest: '<%= distdir %>/js/umbraco.installer.js',
                options: {
                    banner: "<%= banner %>\n(function() { \n\n angular.module('umbraco.install', []); \n",
                    footer: "\n\n})();"
                }
            },
            canvasdesignerJs: {
                src: ['src/canvasdesigner/canvasdesigner.global.js', 'src/canvasdesigner/canvasdesigner.controller.js', 'src/canvasdesigner/editors/*.js', 'src/canvasdesigner/lib/*.js'],
                dest: '<%= distdir %>/js/canvasdesigner.panel.js'
            },
            controllers: {
                src: ['src/controllers/**/*.controller.js', 'src/views/**/*.controller.js'],
                dest: '<%= distdir %>/js/umbraco.controllers.js',
                options: {
                    banner: "<%= banner %>\n(function() { \n\n",
                    footer: "\n\n})();"
                }
            },
            services: {
                src: ['src/common/services/*.js'],
                dest: '<%= distdir %>/js/umbraco.services.js',
                options: {
                    banner: "<%= banner %>\n(function() { \n\n",
                    footer: "\n\n})();"
                }
            },
            security: {
                src: ['src/common/security/*.js'],
                dest: '<%= distdir %>/js/umbraco.security.js',
                options: {
                    banner: "<%= banner %>\n(function() { \n\n",
                    footer: "\n\n})();"
                }
            },
            resources: {
                src: ['src/common/resources/*.js'],
                dest: '<%= distdir %>/js/umbraco.resources.js',
                options: {
                    banner: "<%= banner %>\n(function() { \n\n",
                    footer: "\n\n})();"
                }
            },
            testing: {
                src: ['src/common/mocks/*/*.js'],
                dest: '<%= distdir %>/js/umbraco.testing.js',
                options: {
                    banner: "<%= banner %>\n(function() { \n\n",
                    footer: "\n\n})();"
                }
            },
            directives: {
                src: ['src/common/directives/**/*.js'],
                dest: '<%= distdir %>/js/umbraco.directives.js',
                options: {
                    banner: "<%= banner %>\n(function() { \n\n",
                    footer: "\n\n})();"
                }
            },
            filters: {
                src: ['src/common/filters/*.js'],
                dest: '<%= distdir %>/js/umbraco.filters.js',
                options: {
                    banner: "<%= banner %>\n(function() { \n\n",
                    footer: "\n\n})();"
                }
            }
        },

        uglify: {
            options: {
                mangle: true
            },
            combine: {
                files: {
                    '<%= distdir %>/js/umbraco.min.js': ['<%= distdir %>/js/umbraco.*.js']
                }
            }
        },

        recess: {
            build: {
                files: {
                    '<%= distdir %>/assets/css/<%= pkg.name %>.css':
                    ['<%= src.less %>']
                },
                options: {
                    compile: true
                }
            },
            installer: {
                files: {
                    '<%= distdir %>/assets/css/installer.css':
                    ['src/less/installer.less']
                },
                options: {
                    compile: true
                }
            },
            canvasdesigner: {
                files: {
                    '<%= distdir %>/assets/css/canvasdesigner.css':
                    ['src/less/canvasdesigner.less', 'src/less/helveticons.less']
                },
                options: {
                    compile: true
                }
            },
            min: {
                files: {
                    '<%= distdir %>/assets/css/<%= pkg.name %>.css': ['<%= src.less %>']
                },
                options: {
                    compile: true,
                    compress: true
                }
            }
        },


        watch: {
            css: {
                files: 'src/**/*.less',
                tasks: ['watch-less', 'timestamp'],
                options: {
                    livereload: true,
                },
            },
            js: {
                files: ['src/**/*.js', 'src/*.js'],
                tasks: ['watch-js', 'timestamp'],
            },
            test: {
                files: ['test/**/*.js'],
                tasks: ['watch-test', 'timestamp'],
            },
            installer: {
                files: ['src/installer/**/*.*'],
                tasks: ['watch-installer', 'timestamp'],
            },
            canvasdesigner: {
                files: ['src/canvasdesigner/**/*.*'],
                tasks: ['watch-canvasdesigner', 'timestamp'],
            },
            html: {
                files: ['src/views/**/*.html', 'src/*.html'],
                tasks: ['watch-html', 'timestamp']
            }
        },


        ngdocs: {
            options: {
                dest: 'docs/api',
                startPage: '/api',
                title: "Umbraco 7",
                html5Mode: false,
            },
            api: {
                src: ['src/common/**/*.js', 'docs/src/api/**/*.ngdoc'],
                title: 'API Documentation'
            },
            tutorials: {
                src: ['docs/src/tutorials/**/*.ngdoc'],
                title: 'Tutorials'
            }
        },

        jshint: {
            dev: {
                files: {
                    src: ['<%= src.common %>', '<%= src.specs %>', '<%= src.scenarios %>', '<%= src.samples %>']
                },
                options: {
                    curly: true,
                    eqeqeq: true,
                    immed: true,
                    latedef: true,
                    newcap: true,
                    noarg: true,
                    sub: true,
                    boss: true,
                    //NOTE: This is required so it doesn't barf on reserved words like delete when doing $http.delete
                    es5: true,
                    eqnull: true,
                    //NOTE: we need to use eval sometimes so ignore it
                    evil: true,
                    //NOTE: we need to check for strings such as "javascript:" so don't throw errors regarding those
                    scripturl: true,
                    //NOTE: we ignore tabs vs spaces because enforcing that causes lots of errors depending on the text editor being used
                    smarttabs: true,
                    globals: {}
                }
            },
            build: {
                files: {
                    src: ['<%= src.prod %>']
                },
                options: {
                    curly: true,
                    eqeqeq: true,
                    immed: true,
                    latedef: true,
                    newcap: true,
                    noarg: true,
                    sub: true,
                    boss: true,
                    //NOTE: This is required so it doesn't barf on reserved words like delete when doing $http.delete
                    es5: true,
                    eqnull: true,
                    //NOTE: we need to use eval sometimes so ignore it
                    evil: true,
                    //NOTE: we need to check for strings such as "javascript:" so don't throw errors regarding those
                    scripturl: true,
                    //NOTE: we ignore tabs vs spaces because enforcing that causes lots of errors depending on the text editor being used
                    smarttabs: true,
                    globalstrict: true,
                    globals: { $: false, jQuery: false, define: false, require: false, window: false }
                }
            }
        },

        bower: {
            install: {
                options: {
                    targetDir: "<%= distdir %>/lib",
                    cleanTargetDir: false,
                    layout: function (type, component, source) {

                        var path = require('path');

                        //this is the same as 'byComponent', however we will not allow
                        // folders with '.' in them since the grunt copy task does not like that
                        var componentWithoutPeriod = component.replace(".", "-");
                        return path.join(componentWithoutPeriod, type);
                    }
                }
            }
        }
    });



    grunt.loadNpmTasks('grunt-contrib-concat');
    grunt.loadNpmTasks('grunt-contrib-jshint');
    grunt.loadNpmTasks('grunt-contrib-clean');
    grunt.loadNpmTasks('grunt-contrib-copy');
    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-watch');
    grunt.loadNpmTasks('grunt-recess');

    grunt.loadNpmTasks('grunt-karma');

    grunt.loadNpmTasks('grunt-open');
    grunt.loadNpmTasks('grunt-contrib-connect');
    grunt.loadNpmTasks('grunt-bower-task');
    grunt.loadNpmTasks('grunt-ngdocs');

};
