import { nodeResolve } from '@rollup/plugin-node-resolve';
import css from 'rollup-plugin-import-css';
import replace from '@rollup/plugin-replace';
import { terser } from 'rollup-plugin-terser';
import sass from 'rollup-plugin-sass';
import cssnano from 'cssnano';
import postcss from 'postcss';
import { existsSync } from 'fs';
import { mkdir, writeFile } from 'fs/promises';
import { dirname } from 'path';

export default [
  // ES6 standalone files
  {
    input: 'src/js/sa11y.js',
    plugins: [
      nodeResolve(),
      css(),
      replace({
        preventAssignment: true,
        'process.env.NODE_ENV': JSON.stringify('production'),
      }),
    ],
    output: [
      { file: 'dist/js/sa11y.esm.js', format: 'esm' },
      {
        file: 'dist/js/sa11y.esm.min.js', format: 'esm', plugins: [terser()],
      },
    ],
  },
  // UMD standalone files
  {
    input: 'src/js/sa11y.js',
    plugins: [
      nodeResolve(),
      css(),
      replace({
        preventAssignment: true,
        'process.env.NODE_ENV': JSON.stringify('production'),
      }),
    ],
    output: [
      { file: 'dist/js/sa11y.umd.js', format: 'umd', name: 'Sa11y' },
      {
        file: 'dist/js/sa11y.umd.min.js', format: 'umd', name: 'Sa11y', plugins: [terser()],
      },
    ],
  },
  // Custom checks ESM
  {
    input: 'src/js/sa11y-custom-checks.js',
    plugins: [
      nodeResolve(),
      css(),
      replace({
        preventAssignment: true,
        'process.env.NODE_ENV': JSON.stringify('production'),
      }),
    ],
    output: [
      { file: 'dist/js/sa11y-custom-checks.esm.js', format: 'esm' },
      {
        file: 'dist/js/sa11y-custom-checks.esm.min.js', format: 'esm', plugins: [terser()],
      },
    ],
  },
  // Custom checks
  {
    input: 'src/js/sa11y-custom-checks.js',
    plugins: [
      nodeResolve(),
      css(),
      replace({
        preventAssignment: true,
        'process.env.NODE_ENV': JSON.stringify('production'),
      }),
    ],
    output: [
      { file: 'dist/js/sa11y-custom-checks.umd.js', format: 'umd', name: 'CustomChecks' },
      {
        file: 'dist/js/sa11y-custom-checks.umd.min.js', format: 'umd', name: 'CustomChecks', plugins: [terser()],
      },
    ],
  },
  // Language files
  {
    input: 'src/js/lang/en.js',
    plugins: [nodeResolve()],
    output: [
      { file: 'dist/js/lang/en.js', format: 'esm' },
    ],
  },
  {
    input: 'src/js/lang/en.js',
    plugins: [nodeResolve()],
    output: [
      { file: 'dist/js/lang/en.umd.js', format: 'umd', name: 'Sa11yLangEn' },
    ],
  },
  // French
  {
    input: 'src/js/lang/fr.js',
    plugins: [nodeResolve()],
    output: [
      { file: 'dist/js/lang/fr.js', format: 'esm' },
    ],
  },
  {
    input: 'src/js/lang/fr.js',
    plugins: [nodeResolve()],
    output: [
      { file: 'dist/js/lang/fr.umd.js', format: 'umd', name: 'Sa11yLangFr' },
    ],
  },
  // Polish
  {
    input: 'src/js/lang/pl.js',
    plugins: [nodeResolve()],
    output: [
      { file: 'dist/js/lang/pl.js', format: 'esm' },
    ],
  },
  {
    input: 'src/js/lang/pl.js',
    plugins: [nodeResolve()],
    output: [
      { file: 'dist/js/lang/pl.umd.js', format: 'umd', name: 'Sa11yLangPl' },
    ],
  },
  // Ukrainian
  {
    input: 'src/js/lang/ua.js',
    plugins: [nodeResolve()],
    output: [
      { file: 'dist/js/lang/ua.js', format: 'esm' },
    ],
  },
  {
    input: 'src/js/lang/ua.js',
    plugins: [nodeResolve()],
    output: [
      { file: 'dist/js/lang/ua.umd.js', format: 'umd', name: 'Sa11yLangUa' },
    ],
  },
  {
    input: 'src/js/lang/sv.js',
    plugins: [nodeResolve()],
    output: [
      { file: 'dist/js/lang/sv.js', format: 'esm' },
    ],
  },
  {
    input: 'src/js/lang/sv.js',
    plugins: [nodeResolve()],
    output: [
      { file: 'dist/js/lang/sv.umd.js', format: 'umd', name: 'Sa11yLangSv' },
    ],
  },
  // SCSS files
  {
    input: 'src/scss/sa11y.scss',
    plugins: [sass({
      output: false,
      processor: (css) => {
        postcss()
          .process(css, { from: undefined })
          .then(async (result) => {
            const path = 'dist/css/sa11y.css';
            const pathMin = 'dist/css/sa11y.min.css';

            if (!existsSync(dirname(path))) {
              await mkdir(dirname(path), { recursive: true });
            }
            await writeFile(path, result.css, { encoding: 'utf8' });

            postcss([cssnano])
              .process(result.css, { from: undefined })
              .then(async (result) => {
                if (!existsSync(dirname(pathMin))) {
                  await mkdir(dirname(pathMin), { recursive: true });
                }
                await writeFile(pathMin, result.css, { encoding: 'utf8' });
              });
          });
        return '';
      },
    })],
  },

];
