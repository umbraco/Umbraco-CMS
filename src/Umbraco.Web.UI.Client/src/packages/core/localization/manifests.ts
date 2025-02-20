import type { ManifestLocalization } from './extensions/localization.extension.js';

export const manifests: Array<ManifestLocalization> = [
	{
		type: 'localization',
		alias: 'Umb.Localization.Ar',
		weight: -100,
		name: 'العربية',
		meta: {
			culture: 'ar',
		},
		js: () => import('../../../assets/lang/ar.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Bs',
		weight: -100,
		name: 'Bosanski',
		meta: {
			culture: 'bs',
		},
		js: () => import('../../../assets/lang/bs.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Cs-CZ',
		weight: -100,
		name: 'česky',
		meta: {
			culture: 'cs-cz',
		},
		js: () => import('../../../assets/lang/cs-cz.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Cy-GB',
		weight: -100,
		name: 'Cymraeg (UK)',
		meta: {
			culture: 'cy-gb',
		},
		js: () => import('../../../assets/lang/cy-gb.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Da_DK',
		weight: -100,
		name: 'Dansk (Danmark)',
		meta: {
			culture: 'da-dk',
		},
		js: () => import('../../../assets/lang/da-dk.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.De-DE',
		weight: -100,
		name: 'Deutsch (DE)',
		meta: {
			culture: 'de-de',
		},
		js: () => import('../../../assets/lang/de-de.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.De-CH',
		weight: -100,
		name: 'Deutsch (Schweiz)',
		meta: {
			culture: 'de-ch',
		},
		js: () => import('../../../assets/lang/de-ch.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.En-GB',
		weight: -100,
		name: 'English (UK)',
		meta: {
			culture: 'en',
		},
		js: () => import('../../../assets/lang/en.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.En_US',
		weight: -100,
		name: 'English (US)',
		meta: {
			culture: 'en-us',
		},
		js: () => import('../../../assets/lang/en-us.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Es-ES',
		weight: -100,
		name: 'español',
		meta: {
			culture: 'es-es',
		},
		js: () => import('../../../assets/lang/es-es.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Fr-FR',
		weight: -100,
		name: 'français',
		meta: {
			culture: 'fr-fr',
		},
		js: () => import('../../../assets/lang/fr-fr.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Fr-CH',
		weight: -100,
		name: 'Français (Suisse)',
		meta: {
			culture: 'fr-ch',
		},
		js: () => import('../../../assets/lang/fr-ch.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.He-IL',
		weight: -100,
		name: 'Hebrew',
		meta: {
			culture: 'he-il',
		},
		js: () => import('../../../assets/lang/he-il.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Hr-HR',
		weight: -100,
		name: 'Hrvatski',
		meta: {
			culture: 'hr-hr',
		},
		js: () => import('../../../assets/lang/hr-hr.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.It-IT',
		weight: -100,
		name: 'italiano',
		meta: {
			culture: 'it-it',
		},
		js: () => import('../../../assets/lang/it-it.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.It-CH',
		weight: -100,
		name: 'Italiano (Svizerra)',
		meta: {
			culture: 'it-ch',
		},
		js: () => import('../../../assets/lang/it-ch.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Ja-JP',
		weight: -100,
		name: '日本語',
		meta: {
			culture: 'ja-jp',
		},
		js: () => import('../../../assets/lang/ja-jp.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Ko-KR',
		weight: -100,
		name: '한국어',
		meta: {
			culture: 'ko-kr',
		},
		js: () => import('../../../assets/lang/ko-kr.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Nb-NO',
		weight: -100,
		name: 'norsk',
		meta: {
			culture: 'nb-no',
		},
		js: () => import('../../../assets/lang/nb-no.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Nl-NL',
		weight: -100,
		name: 'Nederlands',
		meta: {
			culture: 'nl-nl',
		},
		js: () => import('../../../assets/lang/nl-nl.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Pl-PL',
		weight: -100,
		name: 'polski',
		meta: {
			culture: 'pl-pl',
		},
		js: () => import('../../../assets/lang/pl-pl.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Pt-BR',
		weight: -100,
		name: 'Portuguese Brazil',
		meta: {
			culture: 'pt-br',
		},
		js: () => import('../../../assets/lang/pt-br.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Ro-RO',
		weight: -100,
		name: 'romana (Romania)',
		meta: {
			culture: 'ro-ro',
		},
		js: () => import('../../../assets/lang/ro-ro.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Ru-RU',
		weight: -100,
		name: 'русский',
		meta: {
			culture: 'ru-ru',
		},
		js: () => import('../../../assets/lang/ru-ru.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Sv-SE',
		weight: -100,
		name: 'Svenska',
		meta: {
			culture: 'sv-se',
		},
		js: () => import('../../../assets/lang/sv-se.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Tr-TR',
		weight: -100,
		name: 'Türkçe',
		meta: {
			culture: 'tr-tr',
		},
		js: () => import('../../../assets/lang/tr-tr.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Uk-UA',
		weight: -100,
		name: 'Українська',
		meta: {
			culture: 'uk-ua',
		},
		js: () => import('../../../assets/lang/uk-ua.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Zh-CN',
		weight: -100,
		name: '中文（简体，中国）',
		meta: {
			culture: 'zh-cn',
		},
		js: () => import('../../../assets/lang/zh-cn.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.Zh-TW',
		weight: -100,
		name: '中文（正體，台灣）',
		meta: {
			culture: 'zh-tw',
		},
		js: () => import('../../../assets/lang/zh-tw.js'),
	},
];
