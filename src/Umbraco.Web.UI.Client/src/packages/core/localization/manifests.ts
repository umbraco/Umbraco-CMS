import type { ManifestLocalization } from './extensions/localization.extension.js';

export const manifests: Array<ManifestLocalization> = [
	{
		type: 'localization',
		alias: 'Umb.Localization.AR',
		weight: -100,
		name: 'Arabic Backoffice UI Localization',
		meta: {
			culture: 'ar',
		},
		js: () => import('../../../assets/lang/ar.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.BS',
		weight: -100,
		name: 'Bosnian Backoffice UI Localization',
		meta: {
			culture: 'bs',
		},
		js: () => import('../../../assets/lang/bs.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.CS_CZ',
		weight: -100,
		name: 'Czech (Czech Republic) Backoffice UI Localization',
		meta: {
			culture: 'cs-CZ',
		},
		js: () => import('../../../assets/lang/cs-cz.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.CY_GB',
		weight: -100,
		name: 'Welsh (United Kingdom) Backoffice UI Localization',
		meta: {
			culture: 'cy-GB',
		},
		js: () => import('../../../assets/lang/cy-gb.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.DA_DK',
		weight: -100,
		name: 'Danish (Denmark) Backoffice UI Localization',
		meta: {
			culture: 'da-DK',
		},
		js: () => import('../../../assets/lang/da-dk.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.DE_DE',
		weight: -100,
		name: 'German (Germany) Backoffice UI Localization',
		meta: {
			culture: 'de-DE',
		},
		js: () => import('../../../assets/lang/de-de.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.DE_CH',
		weight: -100,
		name: 'German (Switzerland) Backoffice UI Localization',
		meta: {
			culture: 'de-CH',
		},
		js: () => import('../../../assets/lang/de-ch.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.EN_GB',
		weight: -100,
		name: 'English (United Kingdom) Backoffice UI Localization',
		meta: {
			culture: 'en',
		},
		js: () => import('../../../assets/lang/en.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.EN_US',
		weight: -100,
		name: 'English (United States) Backoffice UI Localization',
		meta: {
			culture: 'en-US',
		},
		js: () => import('../../../assets/lang/en-us.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.ES_ES',
		weight: -100,
		name: 'Spanish (Spain) Backoffice UI Localization',
		meta: {
			culture: 'es-ES',
		},
		js: () => import('../../../assets/lang/es-es.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.FR_FR',
		weight: -100,
		name: 'French (France) Backoffice UI Localization',
		meta: {
			culture: 'fr-FR',
		},
		js: () => import('../../../assets/lang/fr-fr.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.FR_CH',
		weight: -100,
		name: 'French (Switzerland) Backoffice UI Localization',
		meta: {
			culture: 'fr-CH',
		},
		js: () => import('../../../assets/lang/fr-ch.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.HE_IL',
		weight: -100,
		name: 'Hebrew (Israel) Backoffice UI Localization',
		meta: {
			culture: 'he-IL',
		},
		js: () => import('../../../assets/lang/he-il.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.HR_HR',
		weight: -100,
		name: 'Croatian (Croatia) Backoffice UI Localization',
		meta: {
			culture: 'hr-HR',
		},
		js: () => import('../../../assets/lang/hr-hr.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.IT_IT',
		weight: -100,
		name: 'Italian (Italy) Backoffice UI Localization',
		meta: {
			culture: 'it-IT',
		},
		js: () => import('../../../assets/lang/it-it.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.IT_CH',
		weight: -100,
		name: 'Italian (Switzerland) Backoffice UI Localization',
		meta: {
			culture: 'it-CH',
		},
		js: () => import('../../../assets/lang/it-ch.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.JA_JP',
		weight: -100,
		name: 'Japanese (Japan) Backoffice UI Localization',
		meta: {
			culture: 'ja-JP',
		},
		js: () => import('../../../assets/lang/ja-jp.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.KO_KR',
		weight: -100,
		name: 'Korean (Korea) Backoffice UI Localization',
		meta: {
			culture: 'ko-KR',
		},
		js: () => import('../../../assets/lang/ko-kr.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.NB_NO',
		weight: -100,
		name: 'Norwegian (Norway) Backoffice UI Localization',
		meta: {
			culture: 'nb-NO',
		},
		js: () => import('../../../assets/lang/nb-no.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.NL_NL',
		weight: -100,
		name: 'Dutch (Netherlands) Backoffice UI Localization',
		meta: {
			culture: 'nl-NL',
		},
		js: () => import('../../../assets/lang/nl-nl.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.PL_PL',
		weight: -100,
		name: 'Polish (Poland) Backoffice UI Localization',
		meta: {
			culture: 'pl-PL',
		},
		js: () => import('../../../assets/lang/pl-pl.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.PT_BR',
		weight: -100,
		name: 'Portuguese (Brazil) Backoffice UI Localization',
		meta: {
			culture: 'pt-BR',
		},
		js: () => import('../../../assets/lang/pt-br.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.RO_RO',
		weight: -100,
		name: 'Romanian (Romania) Backoffice UI Localization',
		meta: {
			culture: 'ro-RO',
		},
		js: () => import('../../../assets/lang/ro-ro.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.RU_RU',
		weight: -100,
		name: 'Russian (Russia) Backoffice UI Localization',
		meta: {
			culture: 'ru-RU',
		},
		js: () => import('../../../assets/lang/ru-ru.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.SV_SE',
		weight: -100,
		name: 'Swedish (Sweden) Backoffice UI Localization',
		meta: {
			culture: 'sv-SE',
		},
		js: () => import('../../../assets/lang/sv-se.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.TR_TR',
		weight: -100,
		name: 'Turkish (Turkey) Backoffice UI Localization',
		meta: {
			culture: 'tr-TR',
		},
		js: () => import('../../../assets/lang/tr-tr.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.UK_UA',
		weight: -100,
		name: 'Ukrainian (Ukraine) Backoffice UI Localization',
		meta: {
			culture: 'uk-UA',
		},
		js: () => import('../../../assets/lang/uk-ua.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.ZH_CN',
		weight: -100,
		name: 'Chinese (China) Backoffice UI Localization',
		meta: {
			culture: 'zh-CN',
		},
		js: () => import('../../../assets/lang/zh-cn.js'),
	},
	{
		type: 'localization',
		alias: 'Umb.Localization.ZH_TW',
		weight: -100,
		name: 'Chinese (Taiwan) Backoffice UI Localization',
		meta: {
			culture: 'zh-TW',
		},
		js: () => import('../../../assets/lang/zh-tw.js'),
	},
];
