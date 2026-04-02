import type { ManifestUfmFilter } from '../extensions/ufm-filter.extension.js';

export const manifests: Array<ManifestUfmFilter> = [
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.Fallback',
		name: 'Fallback UFM Filter',
		api: () => import('./fallback.filter.js'),
		meta: { alias: 'fallback' },
	},
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.Bytes',
		name: 'Bytes UFM Filter',
		api: () => import('./bytes.filter.js'),
		meta: { alias: 'bytes' },
	},
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.Lowercase',
		name: 'Lowercase UFM Filter',
		api: () => import('./lowercase.filter.js'),
		meta: { alias: 'lowercase' },
	},
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.StripHtml',
		name: 'Strip HTML UFM Filter',
		api: () => import('./strip-html.filter.js'),
		meta: { alias: 'strip-html' },
	},
	// TODO: Remove in V18 - replaced by camelCase alias below for UFMJS compatibility
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.StripHtmlCamelCase',
		name: 'Strip HTML UFM Filter (camelCase)',
		api: () => import('./strip-html.filter.js'),
		meta: { alias: 'stripHtml' },
	},
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.TitleCase',
		name: 'Title Case UFM Filter',
		api: () => import('./title-case.filter.js'),
		meta: { alias: 'title-case' },
	},
	// TODO: Remove in V18 - replaced by camelCase alias below for UFMJS compatibility
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.TitleCaseCamelCase',
		name: 'Title Case UFM Filter (camelCase)',
		api: () => import('./title-case.filter.js'),
		meta: { alias: 'titleCase' },
	},
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.Truncate',
		name: 'Truncate UFM Filter',
		api: () => import('./truncate.filter.js'),
		meta: { alias: 'truncate' },
	},
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.Uppercase',
		name: 'Uppercase UFM Filter',
		api: () => import('./uppercase.filter.js'),
		meta: { alias: 'uppercase' },
	},
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.WordLimit',
		name: 'Word Limit UFM Filter',
		api: () => import('./word-limit.filter.js'),
		meta: { alias: 'word-limit' },
	},
	// TODO: Remove in V18 - replaced by camelCase alias below for UFMJS compatibility
	{
		type: 'ufmFilter',
		alias: 'Umb.Filter.WordLimitCamelCase',
		name: 'Word Limit UFM Filter (camelCase)',
		api: () => import('./word-limit.filter.js'),
		meta: { alias: 'wordLimit' },
	},
];
