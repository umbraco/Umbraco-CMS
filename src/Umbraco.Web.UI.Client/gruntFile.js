module.exports = function (grunt) {

  // Default task.
  grunt.registerTask('default', ['jshint:dev','build','karma:unit']);
  grunt.registerTask('dev', ['jshint:dev', 'build', 'webserver', 'open:dev', 'watch']);

  //run by the watch task
  grunt.registerTask('watch-js', ['jshint:dev','concat','copy:app','copy:mocks','copy:packages','copy:vs','karma:unit']);
  grunt.registerTask('watch-less', ['recess:build','recess:installer','copy:assets','copy:vs']);
  grunt.registerTask('watch-html', ['copy:views', 'copy:vs']);
  grunt.registerTask('watch-packages', ['copy:packages']);
  grunt.registerTask('watch-installer', ['concat:install','concat:installJs','copy:installer', 'copy:vs']);
  grunt.registerTask('watch-test', ['jshint:dev', 'karma:unit']);

  //triggered from grunt dev or grunt
  grunt.registerTask('build', ['clean','concat','recess:min','recess:installer','copy']);

  //utillity tasks
  grunt.registerTask('docs', ['ngdocs']);
  grunt.registerTask('webserver', ['connect:devserver']);


  // Print a timestamp (useful for when watching)
  grunt.registerTask('timestamp', function() {
    grunt.log.subhead(Date());
  });


  // Project configuration.
  grunt.initConfig({
    connect: {
             devserver: {
               options: {
                 port: 9990,
                 hostname: '0.0.0.0',
                 base: './build',
                 middleware: function(connect, options){
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
             testserver: {}
           },

    open : {
      dev : {
          path: 'http://localhost:9990/belle/'
      }
    },

    distdir: 'build/belle',
    vsdir: '../Umbraco.Web.Ui/umbraco',
    pkg: grunt.file.readJSON('package.json'),
    banner:
    '/*! <%= pkg.title || pkg.name %> - v<%= pkg.version %> - <%= grunt.template.today("yyyy-mm-dd") %>\n' +
    '<%= pkg.homepage ? " * " + pkg.homepage + "\\n" : "" %>' +
    ' * Copyright (c) <%= grunt.template.today("yyyy") %> <%= pkg.author %>;\n' +
    ' * Licensed <%= _.pluck(pkg.licenses, "type").join(", ") %>\n */\n',
    src: {
      js: ['src/**/*.js', 'src/*.js'],
      common: ['src/common/**/*.js'],
      specs: ['test/**/*.spec.js'],
      scenarios: ['test/**/*.scenario.js'],
      samples: ['sample files/*.js'],
      html: ['src/index.html','src/install.html'],

      everything:['src/**/*.*', 'test/**/*.*', 'docs/**/*.*'],

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
        files: [{ dest: '<%= distdir %>/assets', src : '**', expand: true, cwd: 'src/assets/' }]
      },

      installer: {
        files: [{ dest: '<%= distdir %>/views/install', src : '**/*.html', expand: true, cwd: 'src/installer/steps' }]
      },

      vendor: {
        files: [{ dest: '<%= distdir %>/lib', src : '**', expand: true, cwd: 'lib/' }]
      },
      views: {
        files: [{ dest: '<%= distdir %>/views', src : ['**/*.*', '!**/*.controller.js'], expand: true, cwd: 'src/views/' }]
      },
      app: {
        files: [
            { dest: '<%= distdir %>/js', src : '*.js', expand: true, cwd: 'src/' }
            ]
      },
      mocks: {
        files: [{ dest: '<%= distdir %>/js', src : '*.js', expand: true, cwd: 'src/common/mocks/' }]
      },
      vs: {
          files: [
              //everything except the index.html root file!
              //then we need to figure out how to not copy all the test stuff either!?
              { dest: '<%= vsdir %>/assets', src: '**', expand: true, cwd: '<%= distdir %>/assets' },
              { dest: '<%= vsdir %>/js', src: '**', expand: true, cwd: '<%= distdir %>/js' },
              { dest: '<%= vsdir %>/lib', src: '**', expand: true, cwd: '<%= distdir %>/lib' },
              { dest: '<%= vsdir %>/views', src: '**', expand: true, cwd: '<%= distdir %>/views' }
          ]
      },

      packages: {
        files: [{ dest: '<%= vsdir %>/../App_Plugins', src : '**', expand: true, cwd: 'src/packages/' }]
      }
    },

    karma: {
      unit: { configFile: 'test/config/karma.conf.js', keepalive: true },
      e2e: { configFile: 'test/config/e2e.js', keepalive: true },
      watch: { configFile: 'test/config/unit.js', singleRun:false, autoWatch: true, keepalive: true }
    },

    concat:{
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
        controllers: {
          src:['src/controllers/**/*.controller.js','src/views/**/*.controller.js'],
          dest: '<%= distdir %>/js/umbraco.controllers.js',
          options: {
              banner: "<%= banner %>\n(function() { \n\n",
              footer: "\n\n})();"
          }
        },
        services: {
          src:['src/common/services/*.js'],
          dest: '<%= distdir %>/js/umbraco.services.js',
          options: {
              banner: "<%= banner %>\n(function() { \n\n",
              footer: "\n\n})();"
          }
        },
        security: {
          src:['src/common/security/*.js'],
          dest: '<%= distdir %>/js/umbraco.security.js',
          options: {
              banner: "<%= banner %>\n(function() { \n\n",
              footer: "\n\n})();"
          }
        },
        resources: {
          src:['src/common/resources/*.js'],
          dest: '<%= distdir %>/js/umbraco.resources.js',
          options: {
              banner: "<%= banner %>\n(function() { \n\n",
              footer: "\n\n})();"
          }
        },
        testing: {
          src:['src/common/mocks/*/*.js'],
          dest: '<%= distdir %>/js/umbraco.testing.js',
          options: {
              banner: "<%= banner %>\n(function() { \n\n",
              footer: "\n\n})();"
          }
        },
        directives: {
          src:['src/common/directives/**/*.js'],
          dest: '<%= distdir %>/js/umbraco.directives.js',
          options: {
              banner: "<%= banner %>\n(function() { \n\n",
              footer: "\n\n})();"
          }
        },
        filters: {
          src:['src/common/filters/*.js'],
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
          ['<%= src.less %>'] },
        options: {
          compile: true
        }
      },
      installer: {
        files: {
          '<%= distdir %>/assets/css/installer.css':
          ['src/less/installer.less'] },
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


    watch:{
      css: {
          files: '**/*.less',
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
      html: {
        files: ['src/views/**/*.html', 'src/*.html'],
        tasks:['watch-html','timestamp']
      },

      packages: {
          files: 'src/packages/**/*.*',
          tasks: ['watch-packages', 'timestamp'],
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

    jshint:{
      dev:{
         files: {
            src: ['<%= src.common %>', '<%= src.specs %>', '<%= src.scenarios %>', '<%= src.samples %>']
         },
         options:{
           curly:true,
           eqeqeq:true,
           immed:true,
           latedef:true,
           newcap:true,
           noarg:true,
           sub:true,
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
           globals:{}
         }
      },
      build:{
         files: {
          src: ['<%= src.prod %>']
          },
          options:{
           curly:true,
           eqeqeq:true,
           immed:true,
           latedef:true,
           newcap:true,
           noarg:true,
           sub:true,
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
           globalstrict:true,
           globals:{$:false, jQuery:false,define:false,require:false,window:false}
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

  grunt.loadNpmTasks('grunt-ngdocs');

};
