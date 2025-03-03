import type { CultureReponseModel, PagedCultureReponseModel } from '@umbraco-cms/backoffice/external/backend-api';

class UmbCulturesData {
	get(): PagedCultureReponseModel {
		return { total: culturesMock.length, items: culturesMock };
	}
}

export const culturesMock: Array<CultureReponseModel> = [
	{
		name: 'af',
		englishName: 'Afrikaans',
	},
	{
		name: 'af-NA',
		englishName: 'Afrikaans (Namibia)',
	},
	{
		name: 'af-ZA',
		englishName: 'Afrikaans (South Africa)',
	},
	{
		name: 'agq',
		englishName: 'Aghem',
	},
	{
		name: 'agq-CM',
		englishName: 'Aghem (Cameroon)',
	},
	{
		name: 'ain',
		englishName: 'Ainu',
	},
	{
		name: 'ain-JP',
		englishName: 'Ainu (Japan)',
	},
	{
		name: 'ak',
		englishName: 'Akan',
	},
	{
		name: 'ak-GH',
		englishName: 'Akan (Ghana)',
	},
	{
		name: 'sq',
		englishName: 'Albanian',
	},
	{
		name: 'sq-AL',
		englishName: 'Albanian (Albania)',
	},
	{
		name: 'sq-XK',
		englishName: 'Albanian (Kosovo)',
	},
	{
		name: 'sq-MK',
		englishName: 'Albanian (North Macedonia)',
	},
	{
		name: 'am',
		englishName: 'Amharic',
	},
	{
		name: 'am-ET',
		englishName: 'Amharic (Ethiopia)',
	},
	{
		name: 'apw',
		englishName: 'Apache, Western',
	},
	{
		name: 'apw-US',
		englishName: 'Apache, Western (United States)',
	},
	{
		name: 'ar',
		englishName: 'Arabic',
	},
	{
		name: 'ar-DZ',
		englishName: 'Arabic (Algeria)',
	},
	{
		name: 'ar-BH',
		englishName: 'Arabic (Bahrain)',
	},
	{
		name: 'ar-TD',
		englishName: 'Arabic (Chad)',
	},
	{
		name: 'ar-KM',
		englishName: 'Arabic (Comoros)',
	},
	{
		name: 'ar-DJ',
		englishName: 'Arabic (Djibouti)',
	},
	{
		name: 'ar-EG',
		englishName: 'Arabic (Egypt)',
	},
	{
		name: 'ar-ER',
		englishName: 'Arabic (Eritrea)',
	},
	{
		name: 'ar-IQ',
		englishName: 'Arabic (Iraq)',
	},
	{
		name: 'ar-IL',
		englishName: 'Arabic (Israel)',
	},
	{
		name: 'ar-JO',
		englishName: 'Arabic (Jordan)',
	},
	{
		name: 'ar-KW',
		englishName: 'Arabic (Kuwait)',
	},
	{
		name: 'ar-LB',
		englishName: 'Arabic (Lebanon)',
	},
	{
		name: 'ar-LY',
		englishName: 'Arabic (Libya)',
	},
	{
		name: 'ar-MR',
		englishName: 'Arabic (Mauritania)',
	},
	{
		name: 'ar-MA',
		englishName: 'Arabic (Morocco)',
	},
	{
		name: 'ar-OM',
		englishName: 'Arabic (Oman)',
	},
	{
		name: 'ar-PS',
		englishName: 'Arabic (Palestinian Territories)',
	},
	{
		name: 'ar-QA',
		englishName: 'Arabic (Qatar)',
	},
	{
		name: 'ar-SA',
		englishName: 'Arabic (Saudi Arabia)',
	},
	{
		name: 'ar-SO',
		englishName: 'Arabic (Somalia)',
	},
	{
		name: 'ar-SS',
		englishName: 'Arabic (South Sudan)',
	},
	{
		name: 'ar-SD',
		englishName: 'Arabic (Sudan)',
	},
	{
		name: 'ar-SY',
		englishName: 'Arabic (Syria)',
	},
	{
		name: 'ar-TN',
		englishName: 'Arabic (Tunisia)',
	},
	{
		name: 'ar-AE',
		englishName: 'Arabic (United Arab Emirates)',
	},
	{
		name: 'ar-EH',
		englishName: 'Arabic (Western Sahara)',
	},
	{
		name: 'ar-001',
		englishName: 'Arabic (world)',
	},
	{
		name: 'ar-YE',
		englishName: 'Arabic (Yemen)',
	},
	{
		name: 'hy',
		englishName: 'Armenian',
	},
	{
		name: 'hy-AM',
		englishName: 'Armenian (Armenia)',
	},
	{
		name: 'as',
		englishName: 'Assamese',
	},
	{
		name: 'as-IN',
		englishName: 'Assamese (India)',
	},
	{
		name: 'syr',
		englishName: 'Assyrian',
	},
	{
		name: 'syr-IQ',
		englishName: 'Assyrian (Iraq)',
	},
	{
		name: 'syr-SY',
		englishName: 'Assyrian (Syria)',
	},
	{
		name: 'ast',
		englishName: 'Asturian',
	},
	{
		name: 'ast-ES',
		englishName: 'Asturian (Spain)',
	},
	{
		name: 'asa',
		englishName: 'Asu',
	},
	{
		name: 'asa-TZ',
		englishName: 'Asu (Tanzania)',
	},
	{
		name: 'az',
		englishName: 'Azerbaijani',
	},
	{
		name: 'az-Cyrl-AZ',
		englishName: 'Azerbaijani (Cyrillic, Azerbaijan)',
	},
	{
		name: 'az-Cyrl',
		englishName: 'Azerbaijani (Cyrillic)',
	},
	{
		name: 'az-Latn-AZ',
		englishName: 'Azerbaijani (Latin, Azerbaijan)',
	},
	{
		name: 'az-Latn',
		englishName: 'Azerbaijani (Latin)',
	},
	{
		name: 'ksf',
		englishName: 'Bafia',
	},
	{
		name: 'ksf-CM',
		englishName: 'Bafia (Cameroon)',
	},
	{
		name: 'bm',
		englishName: 'Bambara',
	},
	{
		name: 'bm-ML',
		englishName: 'Bambara (Mali)',
	},
	{
		name: 'bn',
		englishName: 'Bangla',
	},
	{
		name: 'bn-BD',
		englishName: 'Bangla (Bangladesh)',
	},
	{
		name: 'bn-IN',
		englishName: 'Bangla (India)',
	},
	{
		name: 'bas',
		englishName: 'Basaa',
	},
	{
		name: 'bas-CM',
		englishName: 'Basaa (Cameroon)',
	},
	{
		name: 'ba',
		englishName: 'Bashkir',
	},
	{
		name: 'ba-RU',
		englishName: 'Bashkir (Russia)',
	},
	{
		name: 'eu',
		englishName: 'Basque',
	},
	{
		name: 'eu-ES',
		englishName: 'Basque (Spain)',
	},
	{
		name: 'be',
		englishName: 'Belarusian',
	},
	{
		name: 'be-BY',
		englishName: 'Belarusian (Belarus)',
	},
	{
		name: 'bem',
		englishName: 'Bemba',
	},
	{
		name: 'bem-ZM',
		englishName: 'Bemba (Zambia)',
	},
	{
		name: 'bez',
		englishName: 'Bena',
	},
	{
		name: 'bez-TZ',
		englishName: 'Bena (Tanzania)',
	},
	{
		name: 'byn',
		englishName: 'Blin',
	},
	{
		name: 'byn-ER',
		englishName: 'Blin (Eritrea)',
	},
	{
		name: 'brx',
		englishName: 'Bodo',
	},
	{
		name: 'brx-IN',
		englishName: 'Bodo (India)',
	},
	{
		name: 'bs',
		englishName: 'Bosnian',
	},
	{
		name: 'bs-Cyrl-BA',
		englishName: 'Bosnian (Cyrillic, Bosnia & Herzegovina)',
	},
	{
		name: 'bs-Cyrl',
		englishName: 'Bosnian (Cyrillic)',
	},
	{
		name: 'bs-Latn-BA',
		englishName: 'Bosnian (Latin, Bosnia & Herzegovina)',
	},
	{
		name: 'bs-Latn',
		englishName: 'Bosnian (Latin)',
	},
	{
		name: 'br',
		englishName: 'Breton',
	},
	{
		name: 'br-FR',
		englishName: 'Breton (France)',
	},
	{
		name: 'bg',
		englishName: 'Bulgarian',
	},
	{
		name: 'bg-BG',
		englishName: 'Bulgarian (Bulgaria)',
	},
	{
		name: 'my',
		englishName: 'Burmese',
	},
	{
		name: 'my-MM',
		englishName: 'Burmese (Myanmar [Burma])',
	},
	{
		name: 'yue',
		englishName: 'Cantonese',
	},
	{
		name: 'yue-Hans',
		englishName: 'Cantonese, Simplified',
	},
	{
		name: 'yue-Hans-CN',
		englishName: 'Cantonese, Simplified (China mainland)',
	},
	{
		name: 'yue-Hant',
		englishName: 'Cantonese, Traditional',
	},
	{
		name: 'yue-Hant-HK',
		englishName: 'Cantonese, Traditional (Hong Kong)',
	},
	{
		name: 'ca',
		englishName: 'Catalan',
	},
	{
		name: 'ca-AD',
		englishName: 'Catalan (Andorra)',
	},
	{
		name: 'ca-FR',
		englishName: 'Catalan (France)',
	},
	{
		name: 'ca-IT',
		englishName: 'Catalan (Italy)',
	},
	{
		name: 'ca-ES',
		englishName: 'Catalan (Spain)',
	},
	{
		name: 'ceb',
		englishName: 'Cebuano',
	},
	{
		name: 'ceb-PH',
		englishName: 'Cebuano (Philippines)',
	},
	{
		name: 'tzm',
		englishName: 'Central Atlas Tamazight',
	},
	{
		name: 'tzm-MA',
		englishName: 'Central Atlas Tamazight (Morocco)',
	},
	{
		name: 'ccp',
		englishName: 'Chakma',
	},
	{
		name: 'ccp-BD',
		englishName: 'Chakma (Bangladesh)',
	},
	{
		name: 'ccp-IN',
		englishName: 'Chakma (India)',
	},
	{
		name: 'ce',
		englishName: 'Chechen',
	},
	{
		name: 'ce-RU',
		englishName: 'Chechen (Russia)',
	},
	{
		name: 'chr',
		englishName: 'Cherokee',
	},
	{
		name: 'chr-US',
		englishName: 'Cherokee (United States)',
	},
	{
		name: 'cgg',
		englishName: 'Chiga',
	},
	{
		name: 'cgg-UG',
		englishName: 'Chiga (Uganda)',
	},
	{
		name: 'zh',
		englishName: 'Chinese',
	},
	{
		name: 'zh-Hans',
		englishName: 'Chinese, Simplified',
	},
	{
		name: 'zh-Hans-CN',
		englishName: 'Chinese, Simplified (China mainland)',
	},
	{
		name: 'zh-Hans-HK',
		englishName: 'Chinese, Simplified (Hong Kong)',
	},
	{
		name: 'zh-Hans-JP',
		englishName: 'Chinese, Simplified (Japan)',
	},
	{
		name: 'zh-Hans-MO',
		englishName: 'Chinese, Simplified (Macao)',
	},
	{
		name: 'zh-Hans-SG',
		englishName: 'Chinese, Simplified (Singapore)',
	},
	{
		name: 'zh-Hant',
		englishName: 'Chinese, Traditional',
	},
	{
		name: 'zh-Hant-CN',
		englishName: 'Chinese, Traditional (China mainland)',
	},
	{
		name: 'zh-Hant-HK',
		englishName: 'Chinese, Traditional (Hong Kong)',
	},
	{
		name: 'zh-Hant-MO',
		englishName: 'Chinese, Traditional (Macao)',
	},
	{
		name: 'zh-Hant-TW',
		englishName: 'Chinese, Traditional (Taiwan)',
	},
	{
		name: 'cv',
		englishName: 'Chuvash',
	},
	{
		name: 'cv-RU',
		englishName: 'Chuvash (Russia)',
	},
	{
		name: 'ksh',
		englishName: 'Colognian',
	},
	{
		name: 'ksh-DE',
		englishName: 'Colognian (Germany)',
	},
	{
		name: 'kw',
		englishName: 'Cornish',
	},
	{
		name: 'kw-GB',
		englishName: 'Cornish (United Kingdom)',
	},
	{
		name: 'co',
		englishName: 'Corsican',
	},
	{
		name: 'co-FR',
		englishName: 'Corsican (France)',
	},
	{
		name: 'hr',
		englishName: 'Croatian',
	},
	{
		name: 'hr-BA',
		englishName: 'Croatian (Bosnia & Herzegovina)',
	},
	{
		name: 'hr-HR',
		englishName: 'Croatian (Croatia)',
	},
	{
		name: 'cs',
		englishName: 'Czech',
	},
	{
		name: 'cs-CZ',
		englishName: 'Czech (Czechia)',
	},
	{
		name: 'da',
		englishName: 'Danish',
	},
	{
		name: 'da-DK',
		englishName: 'Danish (Denmark)',
	},
	{
		name: 'da-GL',
		englishName: 'Danish (Greenland)',
	},
	{
		name: 'dv',
		englishName: 'Dhivehi',
	},
	{
		name: 'dv-MV',
		englishName: 'Dhivehi (Maldives)',
	},
	{
		name: 'doi',
		englishName: 'Dogri',
	},
	{
		name: 'doi-IN',
		englishName: 'Dogri (India)',
	},
	{
		name: 'dua',
		englishName: 'Duala',
	},
	{
		name: 'dua-CM',
		englishName: 'Duala (Cameroon)',
	},
	{
		name: 'nl',
		englishName: 'Dutch',
	},
	{
		name: 'nl-AW',
		englishName: 'Dutch (Aruba)',
	},
	{
		name: 'nl-BE',
		englishName: 'Dutch (Belgium)',
	},
	{
		name: 'nl-BQ',
		englishName: 'Dutch (Caribbean Netherlands)',
	},
	{
		name: 'nl-CW',
		englishName: 'Dutch (Curaçao)',
	},
	{
		name: 'nl-NL',
		englishName: 'Dutch (Netherlands)',
	},
	{
		name: 'nl-SX',
		englishName: 'Dutch (Sint Maarten)',
	},
	{
		name: 'nl-SR',
		englishName: 'Dutch (Suriname)',
	},
	{
		name: 'dz',
		englishName: 'Dzongkha',
	},
	{
		name: 'dz-BT',
		englishName: 'Dzongkha (Bhutan)',
	},
	{
		name: 'ebu',
		englishName: 'Embu',
	},
	{
		name: 'ebu-KE',
		englishName: 'Embu (Kenya)',
	},
	{
		name: 'en',
		englishName: 'English',
	},
	{
		name: 'en-AL',
		englishName: 'English (Albania)',
	},
	{
		name: 'en-AS',
		englishName: 'English (American Samoa)',
	},
	{
		name: 'en-AI',
		englishName: 'English (Anguilla)',
	},
	{
		name: 'en-AG',
		englishName: 'English (Antigua & Barbuda)',
	},
	{
		name: 'en-AR',
		englishName: 'English (Argentina)',
	},
	{
		name: 'en-AU',
		englishName: 'English (Australia)',
	},
	{
		name: 'en-AT',
		englishName: 'English (Austria)',
	},
	{
		name: 'en-BS',
		englishName: 'English (Bahamas)',
	},
	{
		name: 'en-BD',
		englishName: 'English (Bangladesh)',
	},
	{
		name: 'en-BB',
		englishName: 'English (Barbados)',
	},
	{
		name: 'en-BE',
		englishName: 'English (Belgium)',
	},
	{
		name: 'en-BZ',
		englishName: 'English (Belize)',
	},
	{
		name: 'en-BM',
		englishName: 'English (Bermuda)',
	},
	{
		name: 'en-BW',
		englishName: 'English (Botswana)',
	},
	{
		name: 'en-BR',
		englishName: 'English (Brazil)',
	},
	{
		name: 'en-VG',
		englishName: 'English (British Virgin Islands)',
	},
	{
		name: 'en-BN',
		englishName: 'English (Brunei)',
	},
	{
		name: 'en-BG',
		englishName: 'English (Bulgaria)',
	},
	{
		name: 'en-BI',
		englishName: 'English (Burundi)',
	},
	{
		name: 'en-CM',
		englishName: 'English (Cameroon)',
	},
	{
		name: 'en-CA',
		englishName: 'English (Canada)',
	},
	{
		name: 'en-CV',
		englishName: 'English (Cape Verde)',
	},
	{
		name: 'en-KY',
		englishName: 'English (Cayman Islands)',
	},
	{
		name: 'en-IO',
		englishName: 'English (Chagos Archipelago)',
	},
	{
		name: 'en-CL',
		englishName: 'English (Chile)',
	},
	{
		name: 'en-CN',
		englishName: 'English (China mainland)',
	},
	{
		name: 'en-CX',
		englishName: 'English (Christmas Island)',
	},
	{
		name: 'en-CC',
		englishName: 'English (Cocos [Keeling] Islands)',
	},
	{
		name: 'en-CO',
		englishName: 'English (Colombia)',
	},
	{
		name: 'en-CK',
		englishName: 'English (Cook Islands)',
	},
	{
		name: 'en-CY',
		englishName: 'English (Cyprus)',
	},
	{
		name: 'en-CZ',
		englishName: 'English (Czechia)',
	},
	{
		name: 'en-DK',
		englishName: 'English (Denmark)',
	},
	{
		name: 'en-DG',
		englishName: 'English (Diego Garcia)',
	},
	{
		name: 'en-DM',
		englishName: 'English (Dominica)',
	},
	{
		name: 'en-ER',
		englishName: 'English (Eritrea)',
	},
	{
		name: 'en-EE',
		englishName: 'English (Estonia)',
	},
	{
		name: 'en-SZ',
		englishName: 'English (Eswatini)',
	},
	{
		name: 'en-150',
		englishName: 'English (Europe)',
	},
	{
		name: 'en-FK',
		englishName: 'English (Falkland Islands)',
	},
	{
		name: 'en-FJ',
		englishName: 'English (Fiji)',
	},
	{
		name: 'en-FI',
		englishName: 'English (Finland)',
	},
	{
		name: 'en-FR',
		englishName: 'English (France)',
	},
	{
		name: 'en-GM',
		englishName: 'English (Gambia)',
	},
	{
		name: 'en-DE',
		englishName: 'English (Germany)',
	},
	{
		name: 'en-GH',
		englishName: 'English (Ghana)',
	},
	{
		name: 'en-GI',
		englishName: 'English (Gibraltar)',
	},
	{
		name: 'en-GR',
		englishName: 'English (Greece)',
	},
	{
		name: 'en-GD',
		englishName: 'English (Grenada)',
	},
	{
		name: 'en-GU',
		englishName: 'English (Guam)',
	},
	{
		name: 'en-GG',
		englishName: 'English (Guernsey)',
	},
	{
		name: 'en-GY',
		englishName: 'English (Guyana)',
	},
	{
		name: 'en-HK',
		englishName: 'English (Hong Kong)',
	},
	{
		name: 'en-HU',
		englishName: 'English (Hungary)',
	},
	{
		name: 'en-IN',
		englishName: 'English (India)',
	},
	{
		name: 'en-ID',
		englishName: 'English (Indonesia)',
	},
	{
		name: 'en-IE',
		englishName: 'English (Ireland)',
	},
	{
		name: 'en-IM',
		englishName: 'English (Isle of Man)',
	},
	{
		name: 'en-IL',
		englishName: 'English (Israel)',
	},
	{
		name: 'en-JM',
		englishName: 'English (Jamaica)',
	},
	{
		name: 'en-JP',
		englishName: 'English (Japan)',
	},
	{
		name: 'en-JE',
		englishName: 'English (Jersey)',
	},
	{
		name: 'en-KE',
		englishName: 'English (Kenya)',
	},
	{
		name: 'en-KI',
		englishName: 'English (Kiribati)',
	},
	{
		name: 'en-LV',
		englishName: 'English (Latvia)',
	},
	{
		name: 'en-LS',
		englishName: 'English (Lesotho)',
	},
	{
		name: 'en-LR',
		englishName: 'English (Liberia)',
	},
	{
		name: 'en-LT',
		englishName: 'English (Lithuania)',
	},
	{
		name: 'en-MO',
		englishName: 'English (Macao)',
	},
	{
		name: 'en-MG',
		englishName: 'English (Madagascar)',
	},
	{
		name: 'en-MW',
		englishName: 'English (Malawi)',
	},
	{
		name: 'en-MY',
		englishName: 'English (Malaysia)',
	},
	{
		name: 'en-MV',
		englishName: 'English (Maldives)',
	},
	{
		name: 'en-MT',
		englishName: 'English (Malta)',
	},
	{
		name: 'en-MH',
		englishName: 'English (Marshall Islands)',
	},
	{
		name: 'en-MU',
		englishName: 'English (Mauritius)',
	},
	{
		name: 'en-MX',
		englishName: 'English (Mexico)',
	},
	{
		name: 'en-FM',
		englishName: 'English (Micronesia)',
	},
	{
		name: 'en-MS',
		englishName: 'English (Montserrat)',
	},
	{
		name: 'en-MM',
		englishName: 'English (Myanmar [Burma])',
	},
	{
		name: 'en-NA',
		englishName: 'English (Namibia)',
	},
	{
		name: 'en-NR',
		englishName: 'English (Nauru)',
	},
	{
		name: 'en-NL',
		englishName: 'English (Netherlands)',
	},
	{
		name: 'en-NZ',
		englishName: 'English (New Zealand)',
	},
	{
		name: 'en-NG',
		englishName: 'English (Nigeria)',
	},
	{
		name: 'en-NU',
		englishName: 'English (Niue)',
	},
	{
		name: 'en-NF',
		englishName: 'English (Norfolk Island)',
	},
	{
		name: 'en-MP',
		englishName: 'English (Northern Mariana Islands)',
	},
	{
		name: 'en-NO',
		englishName: 'English (Norway)',
	},
	{
		name: 'en-PK',
		englishName: 'English (Pakistan)',
	},
	{
		name: 'en-PW',
		englishName: 'English (Palau)',
	},
	{
		name: 'en-PG',
		englishName: 'English (Papua New Guinea)',
	},
	{
		name: 'en-PH',
		englishName: 'English (Philippines)',
	},
	{
		name: 'en-PN',
		englishName: 'English (Pitcairn Islands)',
	},
	{
		name: 'en-PL',
		englishName: 'English (Poland)',
	},
	{
		name: 'en-PT',
		englishName: 'English (Portugal)',
	},
	{
		name: 'en-PR',
		englishName: 'English (Puerto Rico)',
	},
	{
		name: 'en-RU',
		englishName: 'English (Russia)',
	},
	{
		name: 'en-RW',
		englishName: 'English (Rwanda)',
	},
	{
		name: 'en-WS',
		englishName: 'English (Samoa)',
	},
	{
		name: 'en-SA',
		englishName: 'English (Saudi Arabia)',
	},
	{
		name: 'en-SC',
		englishName: 'English (Seychelles)',
	},
	{
		name: 'en-SL',
		englishName: 'English (Sierra Leone)',
	},
	{
		name: 'en-SG',
		englishName: 'English (Singapore)',
	},
	{
		name: 'en-SX',
		englishName: 'English (Sint Maarten)',
	},
	{
		name: 'en-SK',
		englishName: 'English (Slovakia)',
	},
	{
		name: 'en-SI',
		englishName: 'English (Slovenia)',
	},
	{
		name: 'en-SB',
		englishName: 'English (Solomon Islands)',
	},
	{
		name: 'en-ZA',
		englishName: 'English (South Africa)',
	},
	{
		name: 'en-KR',
		englishName: 'English (South Korea)',
	},
	{
		name: 'en-SS',
		englishName: 'English (South Sudan)',
	},
	{
		name: 'en-SH',
		englishName: 'English (St. Helena)',
	},
	{
		name: 'en-KN',
		englishName: 'English (St. Kitts & Nevis)',
	},
	{
		name: 'en-LC',
		englishName: 'English (St. Lucia)',
	},
	{
		name: 'en-VC',
		englishName: 'English (St. Vincent & Grenadines)',
	},
	{
		name: 'en-SD',
		englishName: 'English (Sudan)',
	},
	{
		name: 'en-SE',
		englishName: 'English (Sweden)',
	},
	{
		name: 'en-CH',
		englishName: 'English (Switzerland)',
	},
	{
		name: 'en-TW',
		englishName: 'English (Taiwan)',
	},
	{
		name: 'en-TZ',
		englishName: 'English (Tanzania)',
	},
	{
		name: 'en-TH',
		englishName: 'English (Thailand)',
	},
	{
		name: 'en-TK',
		englishName: 'English (Tokelau)',
	},
	{
		name: 'en-TO',
		englishName: 'English (Tonga)',
	},
	{
		name: 'en-TT',
		englishName: 'English (Trinidad & Tobago)',
	},
	{
		name: 'en-TR',
		englishName: 'English (Turkey)',
	},
	{
		name: 'en-TC',
		englishName: 'English (Turks & Caicos Islands)',
	},
	{
		name: 'en-TV',
		englishName: 'English (Tuvalu)',
	},
	{
		name: 'en-UM',
		englishName: 'English (U.S. Outlying Islands)',
	},
	{
		name: 'en-VI',
		englishName: 'English (U.S. Virgin Islands)',
	},
	{
		name: 'en-UG',
		englishName: 'English (Uganda)',
	},
	{
		name: 'en-UA',
		englishName: 'English (Ukraine)',
	},
	{
		name: 'en-AE',
		englishName: 'English (United Arab Emirates)',
	},
	{
		name: 'en-GB',
		englishName: 'English (United Kingdom)',
	},
	{
		name: 'en-US-POSIX',
		englishName: 'English (United States, Computer)',
	},
	{
		name: 'en-US',
		englishName: 'English (United States)',
	},
	{
		name: 'en-VU',
		englishName: 'English (Vanuatu)',
	},
	{
		name: 'en-001',
		englishName: 'English (world)',
	},
	{
		name: 'en-ZM',
		englishName: 'English (Zambia)',
	},
	{
		name: 'en-ZW',
		englishName: 'English (Zimbabwe)',
	},
	{
		name: 'myv',
		englishName: 'Erzya',
	},
	{
		name: 'myv-RU',
		englishName: 'Erzya (Russia)',
	},
	{
		name: 'eo',
		englishName: 'Esperanto',
	},
	{
		name: 'eo-001',
		englishName: 'Esperanto (world)',
	},
	{
		name: 'et',
		englishName: 'Estonian',
	},
	{
		name: 'et-EE',
		englishName: 'Estonian (Estonia)',
	},
	{
		name: 'ee',
		englishName: 'Ewe',
	},
	{
		name: 'ee-GH',
		englishName: 'Ewe (Ghana)',
	},
	{
		name: 'ee-TG',
		englishName: 'Ewe (Togo)',
	},
	{
		name: 'ewo',
		englishName: 'Ewondo',
	},
	{
		name: 'ewo-CM',
		englishName: 'Ewondo (Cameroon)',
	},
	{
		name: 'fo',
		englishName: 'Faroese',
	},
	{
		name: 'fo-DK',
		englishName: 'Faroese (Denmark)',
	},
	{
		name: 'fo-FO',
		englishName: 'Faroese (Faroe Islands)',
	},
	{
		name: 'fil',
		englishName: 'Filipino',
	},
	{
		name: 'fil-PH',
		englishName: 'Filipino (Philippines)',
	},
	{
		name: 'fi',
		englishName: 'Finnish',
	},
	{
		name: 'fi-FI',
		englishName: 'Finnish (Finland)',
	},
	{
		name: 'fr',
		englishName: 'French',
	},
	{
		name: 'fr-DZ',
		englishName: 'French (Algeria)',
	},
	{
		name: 'fr-BE',
		englishName: 'French (Belgium)',
	},
	{
		name: 'fr-BJ',
		englishName: 'French (Benin)',
	},
	{
		name: 'fr-BF',
		englishName: 'French (Burkina Faso)',
	},
	{
		name: 'fr-BI',
		englishName: 'French (Burundi)',
	},
	{
		name: 'fr-CM',
		englishName: 'French (Cameroon)',
	},
	{
		name: 'fr-CA',
		englishName: 'French (Canada)',
	},
	{
		name: 'fr-CF',
		englishName: 'French (Central African Republic)',
	},
	{
		name: 'fr-TD',
		englishName: 'French (Chad)',
	},
	{
		name: 'fr-KM',
		englishName: 'French (Comoros)',
	},
	{
		name: 'fr-CG',
		englishName: 'French (Congo - Brazzaville)',
	},
	{
		name: 'fr-CD',
		englishName: 'French (Congo - Kinshasa)',
	},
	{
		name: 'fr-CI',
		englishName: 'French (Côte d’Ivoire)',
	},
	{
		name: 'fr-DJ',
		englishName: 'French (Djibouti)',
	},
	{
		name: 'fr-GQ',
		englishName: 'French (Equatorial Guinea)',
	},
	{
		name: 'fr-FR',
		englishName: 'French (France)',
	},
	{
		name: 'fr-GF',
		englishName: 'French (French Guiana)',
	},
	{
		name: 'fr-PF',
		englishName: 'French (French Polynesia)',
	},
	{
		name: 'fr-GA',
		englishName: 'French (Gabon)',
	},
	{
		name: 'fr-GP',
		englishName: 'French (Guadeloupe)',
	},
	{
		name: 'fr-GN',
		englishName: 'French (Guinea)',
	},
	{
		name: 'fr-HT',
		englishName: 'French (Haiti)',
	},
	{
		name: 'fr-LU',
		englishName: 'French (Luxembourg)',
	},
	{
		name: 'fr-MG',
		englishName: 'French (Madagascar)',
	},
	{
		name: 'fr-ML',
		englishName: 'French (Mali)',
	},
	{
		name: 'fr-MQ',
		englishName: 'French (Martinique)',
	},
	{
		name: 'fr-MR',
		englishName: 'French (Mauritania)',
	},
	{
		name: 'fr-MU',
		englishName: 'French (Mauritius)',
	},
	{
		name: 'fr-YT',
		englishName: 'French (Mayotte)',
	},
	{
		name: 'fr-MC',
		englishName: 'French (Monaco)',
	},
	{
		name: 'fr-MA',
		englishName: 'French (Morocco)',
	},
	{
		name: 'fr-NC',
		englishName: 'French (New Caledonia)',
	},
	{
		name: 'fr-NE',
		englishName: 'French (Niger)',
	},
	{
		name: 'fr-RE',
		englishName: 'French (Réunion)',
	},
	{
		name: 'fr-RW',
		englishName: 'French (Rwanda)',
	},
	{
		name: 'fr-SN',
		englishName: 'French (Senegal)',
	},
	{
		name: 'fr-SC',
		englishName: 'French (Seychelles)',
	},
	{
		name: 'fr-BL',
		englishName: 'French (St. Barthélemy)',
	},
	{
		name: 'fr-MF',
		englishName: 'French (St. Martin)',
	},
	{
		name: 'fr-PM',
		englishName: 'French (St. Pierre & Miquelon)',
	},
	{
		name: 'fr-CH',
		englishName: 'French (Switzerland)',
	},
	{
		name: 'fr-SY',
		englishName: 'French (Syria)',
	},
	{
		name: 'fr-TG',
		englishName: 'French (Togo)',
	},
	{
		name: 'fr-TN',
		englishName: 'French (Tunisia)',
	},
	{
		name: 'fr-VU',
		englishName: 'French (Vanuatu)',
	},
	{
		name: 'fr-WF',
		englishName: 'French (Wallis & Futuna)',
	},
	{
		name: 'fur',
		englishName: 'Friulian',
	},
	{
		name: 'fur-IT',
		englishName: 'Friulian (Italy)',
	},
	{
		name: 'ff',
		englishName: 'Fula',
	},
	{
		name: 'ff-Adlm-BF',
		englishName: 'Fula (Adlam, Burkina Faso)',
	},
	{
		name: 'ff-Adlm-CM',
		englishName: 'Fula (Adlam, Cameroon)',
	},
	{
		name: 'ff-Adlm-GM',
		englishName: 'Fula (Adlam, Gambia)',
	},
	{
		name: 'ff-Adlm-GH',
		englishName: 'Fula (Adlam, Ghana)',
	},
	{
		name: 'ff-Adlm-GW',
		englishName: 'Fula (Adlam, Guinea-Bissau)',
	},
	{
		name: 'ff-Adlm-GN',
		englishName: 'Fula (Adlam, Guinea)',
	},
	{
		name: 'ff-Adlm-LR',
		englishName: 'Fula (Adlam, Liberia)',
	},
	{
		name: 'ff-Adlm-MR',
		englishName: 'Fula (Adlam, Mauritania)',
	},
	{
		name: 'ff-Adlm-NE',
		englishName: 'Fula (Adlam, Niger)',
	},
	{
		name: 'ff-Adlm-NG',
		englishName: 'Fula (Adlam, Nigeria)',
	},
	{
		name: 'ff-Adlm-SN',
		englishName: 'Fula (Adlam, Senegal)',
	},
	{
		name: 'ff-Adlm-SL',
		englishName: 'Fula (Adlam, Sierra Leone)',
	},
	{
		name: 'ff-Adlm',
		englishName: 'Fula (Adlam)',
	},
	{
		name: 'ff-Latn-BF',
		englishName: 'Fula (Latin, Burkina Faso)',
	},
	{
		name: 'ff-Latn-CM',
		englishName: 'Fula (Latin, Cameroon)',
	},
	{
		name: 'ff-Latn-GM',
		englishName: 'Fula (Latin, Gambia)',
	},
	{
		name: 'ff-Latn-GH',
		englishName: 'Fula (Latin, Ghana)',
	},
	{
		name: 'ff-Latn-GW',
		englishName: 'Fula (Latin, Guinea-Bissau)',
	},
	{
		name: 'ff-Latn-GN',
		englishName: 'Fula (Latin, Guinea)',
	},
	{
		name: 'ff-Latn-LR',
		englishName: 'Fula (Latin, Liberia)',
	},
	{
		name: 'ff-Latn-MR',
		englishName: 'Fula (Latin, Mauritania)',
	},
	{
		name: 'ff-Latn-NE',
		englishName: 'Fula (Latin, Niger)',
	},
	{
		name: 'ff-Latn-NG',
		englishName: 'Fula (Latin, Nigeria)',
	},
	{
		name: 'ff-Latn-SN',
		englishName: 'Fula (Latin, Senegal)',
	},
	{
		name: 'ff-Latn-SL',
		englishName: 'Fula (Latin, Sierra Leone)',
	},
	{
		name: 'ff-Latn',
		englishName: 'Fula (Latin)',
	},
	{
		name: 'gaa',
		englishName: 'Ga',
	},
	{
		name: 'gaa-GH',
		englishName: 'Ga (Ghana)',
	},
	{
		name: 'gl',
		englishName: 'Galician',
	},
	{
		name: 'gl-ES',
		englishName: 'Galician (Spain)',
	},
	{
		name: 'lg',
		englishName: 'Ganda',
	},
	{
		name: 'lg-UG',
		englishName: 'Ganda (Uganda)',
	},
	{
		name: 'gez',
		englishName: 'Geez',
	},
	{
		name: 'gez-ER',
		englishName: 'Geez (Eritrea)',
	},
	{
		name: 'gez-ET',
		englishName: 'Geez (Ethiopia)',
	},
	{
		name: 'ka',
		englishName: 'Georgian',
	},
	{
		name: 'ka-GE',
		englishName: 'Georgian (Georgia)',
	},
	{
		name: 'de',
		englishName: 'German',
	},
	{
		name: 'de-AT',
		englishName: 'German (Austria)',
	},
	{
		name: 'de-BE',
		englishName: 'German (Belgium)',
	},
	{
		name: 'de-DE',
		englishName: 'German (Germany)',
	},
	{
		name: 'de-IT',
		englishName: 'German (Italy)',
	},
	{
		name: 'de-LI',
		englishName: 'German (Liechtenstein)',
	},
	{
		name: 'de-LU',
		englishName: 'German (Luxembourg)',
	},
	{
		name: 'de-CH',
		englishName: 'German (Switzerland)',
	},
	{
		name: 'el',
		englishName: 'Greek',
	},
	{
		name: 'el-CY',
		englishName: 'Greek (Cyprus)',
	},
	{
		name: 'el-GR',
		englishName: 'Greek (Greece)',
	},
	{
		name: 'gn',
		englishName: 'Guarani',
	},
	{
		name: 'gn-PY',
		englishName: 'Guarani (Paraguay)',
	},
	{
		name: 'gu',
		englishName: 'Gujarati',
	},
	{
		name: 'gu-IN',
		englishName: 'Gujarati (India)',
	},
	{
		name: 'guz',
		englishName: 'Gusii',
	},
	{
		name: 'guz-KE',
		englishName: 'Gusii (Kenya)',
	},
	{
		name: 'ha',
		englishName: 'Hausa',
	},
	{
		name: 'ha-GH',
		englishName: 'Hausa (Ghana)',
	},
	{
		name: 'ha-NE',
		englishName: 'Hausa (Niger)',
	},
	{
		name: 'ha-NG',
		englishName: 'Hausa (Nigeria)',
	},
	{
		name: 'haw',
		englishName: 'Hawaiian',
	},
	{
		name: 'haw-US',
		englishName: 'Hawaiian (United States)',
	},
	{
		name: 'he',
		englishName: 'Hebrew',
	},
	{
		name: 'he-IL',
		englishName: 'Hebrew (Israel)',
	},
	{
		name: 'hi',
		englishName: 'Hindi',
	},
	{
		name: 'hi-IN',
		englishName: 'Hindi (India)',
	},
	{
		name: 'hi-Latn-IN',
		englishName: 'Hindi (Latin, India)',
	},
	{
		name: 'hi-Latn',
		englishName: 'Hindi (Latin)',
	},
	{
		name: 'hu',
		englishName: 'Hungarian',
	},
	{
		name: 'hu-HU',
		englishName: 'Hungarian (Hungary)',
	},
	{
		name: 'is',
		englishName: 'Icelandic',
	},
	{
		name: 'is-IS',
		englishName: 'Icelandic (Iceland)',
	},
	{
		name: 'io',
		englishName: 'Ido',
	},
	{
		name: 'io-001',
		englishName: 'Ido (world)',
	},
	{
		name: 'ig',
		englishName: 'Igbo',
	},
	{
		name: 'ig-NG',
		englishName: 'Igbo (Nigeria)',
	},
	{
		name: 'smn',
		englishName: 'Inari Sami',
	},
	{
		name: 'smn-FI',
		englishName: 'Inari Sami (Finland)',
	},
	{
		name: 'id',
		englishName: 'Indonesian',
	},
	{
		name: 'id-ID',
		englishName: 'Indonesian (Indonesia)',
	},
	{
		name: 'ia',
		englishName: 'Interlingua',
	},
	{
		name: 'ia-001',
		englishName: 'Interlingua (world)',
	},
	{
		name: 'iu',
		englishName: 'Inuktitut',
	},
	{
		name: 'iu-CA',
		englishName: 'Inuktitut (Canada)',
	},
	{
		name: '',
		englishName: 'Invariant Language (Invariant Country)',
	},
	{
		name: 'ga',
		englishName: 'Irish',
	},
	{
		name: 'ga-IE',
		englishName: 'Irish (Ireland)',
	},
	{
		name: 'ga-GB',
		englishName: 'Irish (United Kingdom)',
	},
	{
		name: 'it',
		englishName: 'Italian',
	},
	{
		name: 'it-IT',
		englishName: 'Italian (Italy)',
	},
	{
		name: 'it-SM',
		englishName: 'Italian (San Marino)',
	},
	{
		name: 'it-CH',
		englishName: 'Italian (Switzerland)',
	},
	{
		name: 'it-VA',
		englishName: 'Italian (Vatican City)',
	},
	{
		name: 'ja',
		englishName: 'Japanese',
	},
	{
		name: 'ja-JP',
		englishName: 'Japanese (Japan)',
	},
	{
		name: 'jv',
		englishName: 'Javanese',
	},
	{
		name: 'jv-ID',
		englishName: 'Javanese (Indonesia)',
	},
	{
		name: 'kaj',
		englishName: 'Jju',
	},
	{
		name: 'kaj-NG',
		englishName: 'Jju (Nigeria)',
	},
	{
		name: 'dyo',
		englishName: 'Jola-Fonyi',
	},
	{
		name: 'dyo-SN',
		englishName: 'Jola-Fonyi (Senegal)',
	},
	{
		name: 'kea',
		englishName: 'Kabuverdianu',
	},
	{
		name: 'kea-CV',
		englishName: 'Kabuverdianu (Cape Verde)',
	},
	{
		name: 'kab',
		englishName: 'Kabyle',
	},
	{
		name: 'kab-DZ',
		englishName: 'Kabyle (Algeria)',
	},
	{
		name: 'kkj',
		englishName: 'Kako',
	},
	{
		name: 'kkj-CM',
		englishName: 'Kako (Cameroon)',
	},
	{
		name: 'kl',
		englishName: 'Kalaallisut',
	},
	{
		name: 'kl-GL',
		englishName: 'Kalaallisut (Greenland)',
	},
	{
		name: 'kln',
		englishName: 'Kalenjin',
	},
	{
		name: 'kln-KE',
		englishName: 'Kalenjin (Kenya)',
	},
	{
		name: 'kam',
		englishName: 'Kamba',
	},
	{
		name: 'kam-KE',
		englishName: 'Kamba (Kenya)',
	},
	{
		name: 'kn',
		englishName: 'Kannada',
	},
	{
		name: 'kn-IN',
		englishName: 'Kannada (India)',
	},
	{
		name: 'ks',
		englishName: 'Kashmiri',
	},
	{
		name: 'ks-Deva-IN',
		englishName: 'Kashmiri (Devanagari, India)',
	},
	{
		name: 'ks-Deva',
		englishName: 'Kashmiri (Devanagari)',
	},
	{
		name: 'ks-Arab-IN',
		englishName: 'Kashmiri (Naskh, India)',
	},
	{
		name: 'ks-Arab',
		englishName: 'Kashmiri (Naskh)',
	},
	{
		name: 'ks-Aran-IN',
		englishName: 'Kashmiri (Nastaliq, India)',
	},
	{
		name: 'kk',
		englishName: 'Kazakh',
	},
	{
		name: 'kk-KZ',
		englishName: 'Kazakh (Kazakhstan)',
	},
	{
		name: 'km',
		englishName: 'Khmer',
	},
	{
		name: 'km-KH',
		englishName: 'Khmer (Cambodia)',
	},
	{
		name: 'ki',
		englishName: 'Kikuyu',
	},
	{
		name: 'ki-KE',
		englishName: 'Kikuyu (Kenya)',
	},
	{
		name: 'rw',
		englishName: 'Kinyarwanda',
	},
	{
		name: 'rw-RW',
		englishName: 'Kinyarwanda (Rwanda)',
	},
	{
		name: 'kok',
		englishName: 'Konkani',
	},
	{
		name: 'kok-IN',
		englishName: 'Konkani (India)',
	},
	{
		name: 'ko',
		englishName: 'Korean',
	},
	{
		name: 'ko-KP',
		englishName: 'Korean (North Korea)',
	},
	{
		name: 'ko-KR',
		englishName: 'Korean (South Korea)',
	},
	{
		name: 'khq',
		englishName: 'Koyra Chiini',
	},
	{
		name: 'khq-ML',
		englishName: 'Koyra Chiini (Mali)',
	},
	{
		name: 'ses',
		englishName: 'Koyraboro Senni',
	},
	{
		name: 'ses-ML',
		englishName: 'Koyraboro Senni (Mali)',
	},
	{
		name: 'kpe',
		englishName: 'Kpelle',
	},
	{
		name: 'kpe-GN',
		englishName: 'Kpelle (Guinea)',
	},
	{
		name: 'kpe-LR',
		englishName: 'Kpelle (Liberia)',
	},
	{
		name: 'ku',
		englishName: 'Kurdish',
	},
	{
		name: 'ku-TR',
		englishName: 'Kurdish (Turkey)',
	},
	{
		name: 'ckb',
		englishName: 'Kurdish, Sorani',
	},
	{
		name: 'ckb-IR',
		englishName: 'Kurdish, Sorani (Iran)',
	},
	{
		name: 'ckb-IQ',
		englishName: 'Kurdish, Sorani (Iraq)',
	},
	{
		name: 'nmg',
		englishName: 'Kwasio',
	},
	{
		name: 'nmg-CM',
		englishName: 'Kwasio (Cameroon)',
	},
	{
		name: 'ky',
		englishName: 'Kyrgyz',
	},
	{
		name: 'ky-KG',
		englishName: 'Kyrgyz (Kyrgyzstan)',
	},
	{
		name: 'lkt',
		englishName: 'Lakota',
	},
	{
		name: 'lkt-US',
		englishName: 'Lakota (United States)',
	},
	{
		name: 'lag',
		englishName: 'Langi',
	},
	{
		name: 'lag-TZ',
		englishName: 'Langi (Tanzania)',
	},
	{
		name: 'lo',
		englishName: 'Lao',
	},
	{
		name: 'lo-LA',
		englishName: 'Lao (Laos)',
	},
	{
		name: 'lv',
		englishName: 'Latvian',
	},
	{
		name: 'lv-LV',
		englishName: 'Latvian (Latvia)',
	},
	{
		name: 'ln',
		englishName: 'Lingala',
	},
	{
		name: 'ln-AO',
		englishName: 'Lingala (Angola)',
	},
	{
		name: 'ln-CF',
		englishName: 'Lingala (Central African Republic)',
	},
	{
		name: 'ln-CG',
		englishName: 'Lingala (Congo - Brazzaville)',
	},
	{
		name: 'ln-CD',
		englishName: 'Lingala (Congo - Kinshasa)',
	},
	{
		name: 'lt',
		englishName: 'Lithuanian',
	},
	{
		name: 'lt-LT',
		englishName: 'Lithuanian (Lithuania)',
	},
	{
		name: 'jbo',
		englishName: 'Lojban',
	},
	{
		name: 'jbo-001',
		englishName: 'Lojban (world)',
	},
	{
		name: 'nds',
		englishName: 'Low German',
	},
	{
		name: 'nds-DE',
		englishName: 'Low German (Germany)',
	},
	{
		name: 'nds-NL',
		englishName: 'Low German (Netherlands)',
	},
	{
		name: 'dsb',
		englishName: 'Lower Sorbian',
	},
	{
		name: 'dsb-DE',
		englishName: 'Lower Sorbian (Germany)',
	},
	{
		name: 'lu',
		englishName: 'Luba-Katanga',
	},
	{
		name: 'lu-CD',
		englishName: 'Luba-Katanga (Congo - Kinshasa)',
	},
	{
		name: 'luo',
		englishName: 'Luo',
	},
	{
		name: 'luo-KE',
		englishName: 'Luo (Kenya)',
	},
	{
		name: 'lb',
		englishName: 'Luxembourgish',
	},
	{
		name: 'lb-LU',
		englishName: 'Luxembourgish (Luxembourg)',
	},
	{
		name: 'luy',
		englishName: 'Luyia',
	},
	{
		name: 'luy-KE',
		englishName: 'Luyia (Kenya)',
	},
	{
		name: 'mk',
		englishName: 'Macedonian',
	},
	{
		name: 'mk-MK',
		englishName: 'Macedonian (North Macedonia)',
	},
	{
		name: 'jmc',
		englishName: 'Machame',
	},
	{
		name: 'jmc-TZ',
		englishName: 'Machame (Tanzania)',
	},
	{
		name: 'mai',
		englishName: 'Maithili',
	},
	{
		name: 'mai-IN',
		englishName: 'Maithili (India)',
	},
	{
		name: 'mgh',
		englishName: 'Makhuwa-Meetto',
	},
	{
		name: 'mgh-MZ',
		englishName: 'Makhuwa-Meetto (Mozambique)',
	},
	{
		name: 'kde',
		englishName: 'Makonde',
	},
	{
		name: 'kde-TZ',
		englishName: 'Makonde (Tanzania)',
	},
	{
		name: 'mg',
		englishName: 'Malagasy',
	},
	{
		name: 'mg-MG',
		englishName: 'Malagasy (Madagascar)',
	},
	{
		name: 'ms',
		englishName: 'Malay',
	},
	{
		name: 'ms-Arab-BN',
		englishName: 'Malay (Arabic, Brunei)',
	},
	{
		name: 'ms-Arab-MY',
		englishName: 'Malay (Arabic, Malaysia)',
	},
	{
		name: 'ms-Arab',
		englishName: 'Malay (Arabic)',
	},
	{
		name: 'ms-BN',
		englishName: 'Malay (Brunei)',
	},
	{
		name: 'ms-ID',
		englishName: 'Malay (Indonesia)',
	},
	{
		name: 'ms-MY',
		englishName: 'Malay (Malaysia)',
	},
	{
		name: 'ms-SG',
		englishName: 'Malay (Singapore)',
	},
	{
		name: 'ml',
		englishName: 'Malayalam',
	},
	{
		name: 'ml-IN',
		englishName: 'Malayalam (India)',
	},
	{
		name: 'mt',
		englishName: 'Maltese',
	},
	{
		name: 'mt-MT',
		englishName: 'Maltese (Malta)',
	},
	{
		name: 'mni',
		englishName: 'Manipuri',
	},
	{
		name: 'mni-Beng-IN',
		englishName: 'Manipuri (Bangla, India)',
	},
	{
		name: 'mni-Beng',
		englishName: 'Manipuri (Bangla)',
	},
	{
		name: 'mni-Mtei-IN',
		englishName: 'Manipuri (Meitei Mayek, India)',
	},
	{
		name: 'mni-Mtei',
		englishName: 'Manipuri (Meitei Mayek)',
	},
	{
		name: 'gv',
		englishName: 'Manx',
	},
	{
		name: 'gv-IM',
		englishName: 'Manx (Isle of Man)',
	},
	{
		name: 'mi',
		englishName: 'Māori',
	},
	{
		name: 'mi-NZ',
		englishName: 'Māori (New Zealand)',
	},
	{
		name: 'arn',
		englishName: 'Mapuche',
	},
	{
		name: 'arn-CL',
		englishName: 'Mapuche (Chile)',
	},
	{
		name: 'mr',
		englishName: 'Marathi',
	},
	{
		name: 'mr-IN',
		englishName: 'Marathi (India)',
	},
	{
		name: 'mas',
		englishName: 'Masai',
	},
	{
		name: 'mas-KE',
		englishName: 'Masai (Kenya)',
	},
	{
		name: 'mas-TZ',
		englishName: 'Masai (Tanzania)',
	},
	{
		name: 'mzn',
		englishName: 'Mazanderani',
	},
	{
		name: 'mzn-IR',
		englishName: 'Mazanderani (Iran)',
	},
	{
		name: 'mer',
		englishName: 'Meru',
	},
	{
		name: 'mer-KE',
		englishName: 'Meru (Kenya)',
	},
	{
		name: 'mgo',
		englishName: 'Metaʼ',
	},
	{
		name: 'mgo-CM',
		englishName: 'Metaʼ (Cameroon)',
	},
	{
		name: 'moh',
		englishName: 'Mohawk',
	},
	{
		name: 'moh-CA',
		englishName: 'Mohawk (Canada)',
	},
	{
		name: 'mn',
		englishName: 'Mongolian',
	},
	{
		name: 'mn-MN',
		englishName: 'Mongolian (Mongolia)',
	},
	{
		name: 'mfe',
		englishName: 'Morisyen',
	},
	{
		name: 'mfe-MU',
		englishName: 'Morisyen (Mauritius)',
	},
	{
		name: 'mua',
		englishName: 'Mundang',
	},
	{
		name: 'mua-CM',
		englishName: 'Mundang (Cameroon)',
	},
	{
		name: 'nqo',
		englishName: 'N’Ko',
	},
	{
		name: 'nqo-GN',
		englishName: 'N’Ko (Guinea)',
	},
	{
		name: 'naq',
		englishName: 'Nama',
	},
	{
		name: 'naq-NA',
		englishName: 'Nama (Namibia)',
	},
	{
		name: 'nv',
		englishName: 'Navajo',
	},
	{
		name: 'nv-US',
		englishName: 'Navajo (United States)',
	},
	{
		name: 'ne',
		englishName: 'Nepali',
	},
	{
		name: 'ne-IN',
		englishName: 'Nepali (India)',
	},
	{
		name: 'ne-NP',
		englishName: 'Nepali (Nepal)',
	},
	{
		name: 'nnh',
		englishName: 'Ngiemboon',
	},
	{
		name: 'nnh-CM',
		englishName: 'Ngiemboon (Cameroon)',
	},
	{
		name: 'jgo',
		englishName: 'Ngomba',
	},
	{
		name: 'jgo-CM',
		englishName: 'Ngomba (Cameroon)',
	},
	{
		name: 'pcm',
		englishName: 'Nigerian Pidgin',
	},
	{
		name: 'pcm-NG',
		englishName: 'Nigerian Pidgin (Nigeria)',
	},
	{
		name: 'nd',
		englishName: 'North Ndebele',
	},
	{
		name: 'nd-ZW',
		englishName: 'North Ndebele (Zimbabwe)',
	},
	{
		name: 'lrc',
		englishName: 'Northern Luri',
	},
	{
		name: 'lrc-IR',
		englishName: 'Northern Luri (Iran)',
	},
	{
		name: 'lrc-IQ',
		englishName: 'Northern Luri (Iraq)',
	},
	{
		name: 'se',
		englishName: 'Northern Sami',
	},
	{
		name: 'se-FI',
		englishName: 'Northern Sami (Finland)',
	},
	{
		name: 'se-NO',
		englishName: 'Northern Sami (Norway)',
	},
	{
		name: 'se-SE',
		englishName: 'Northern Sami (Sweden)',
	},
	{
		name: 'nso',
		englishName: 'Northern Sotho',
	},
	{
		name: 'nso-ZA',
		englishName: 'Northern Sotho (South Africa)',
	},
	{
		name: 'no',
		englishName: 'Norwegian',
	},
	{
		name: 'nb',
		englishName: 'Norwegian Bokmål',
	},
	{
		name: 'nb-NO',
		englishName: 'Norwegian Bokmål (Norway)',
	},
	{
		name: 'nb-SJ',
		englishName: 'Norwegian Bokmål (Svalbard & Jan Mayen)',
	},
	{
		name: 'nn',
		englishName: 'Norwegian Nynorsk',
	},
	{
		name: 'nn-NO',
		englishName: 'Norwegian Nynorsk (Norway)',
	},
	{
		name: 'nus',
		englishName: 'Nuer',
	},
	{
		name: 'nus-SS',
		englishName: 'Nuer (South Sudan)',
	},
	{
		name: 'ny',
		englishName: 'Nyanja',
	},
	{
		name: 'ny-MW',
		englishName: 'Nyanja (Malawi)',
	},
	{
		name: 'nyn',
		englishName: 'Nyankole',
	},
	{
		name: 'nyn-UG',
		englishName: 'Nyankole (Uganda)',
	},
	{
		name: 'oc',
		englishName: 'Occitan',
	},
	{
		name: 'oc-FR',
		englishName: 'Occitan (France)',
	},
	{
		name: 'or',
		englishName: 'Odia',
	},
	{
		name: 'or-IN',
		englishName: 'Odia (India)',
	},
	{
		name: 'om',
		englishName: 'Oromo',
	},
	{
		name: 'om-ET',
		englishName: 'Oromo (Ethiopia)',
	},
	{
		name: 'om-KE',
		englishName: 'Oromo (Kenya)',
	},
	{
		name: 'os',
		englishName: 'Ossetic',
	},
	{
		name: 'os-GE',
		englishName: 'Ossetic (Georgia)',
	},
	{
		name: 'os-RU',
		englishName: 'Ossetic (Russia)',
	},
	{
		name: 'ps',
		englishName: 'Pashto',
	},
	{
		name: 'ps-AF',
		englishName: 'Pashto (Afghanistan)',
	},
	{
		name: 'ps-PK',
		englishName: 'Pashto (Pakistan)',
	},
	{
		name: 'fa',
		englishName: 'Persian',
	},
	{
		name: 'fa-AF',
		englishName: 'Persian (Afghanistan)',
	},
	{
		name: 'fa-IR',
		englishName: 'Persian (Iran)',
	},
	{
		name: 'pl',
		englishName: 'Polish',
	},
	{
		name: 'pl-PL',
		englishName: 'Polish (Poland)',
	},
	{
		name: 'pt',
		englishName: 'Portuguese',
	},
	{
		name: 'pt-AO',
		englishName: 'Portuguese (Angola)',
	},
	{
		name: 'pt-BR',
		englishName: 'Portuguese (Brazil)',
	},
	{
		name: 'pt-CV',
		englishName: 'Portuguese (Cape Verde)',
	},
	{
		name: 'pt-GQ',
		englishName: 'Portuguese (Equatorial Guinea)',
	},
	{
		name: 'pt-FR',
		englishName: 'Portuguese (France)',
	},
	{
		name: 'pt-GW',
		englishName: 'Portuguese (Guinea-Bissau)',
	},
	{
		name: 'pt-LU',
		englishName: 'Portuguese (Luxembourg)',
	},
	{
		name: 'pt-MO',
		englishName: 'Portuguese (Macao)',
	},
	{
		name: 'pt-MZ',
		englishName: 'Portuguese (Mozambique)',
	},
	{
		name: 'pt-PT',
		englishName: 'Portuguese (Portugal)',
	},
	{
		name: 'pt-ST',
		englishName: 'Portuguese (São Tomé & Príncipe)',
	},
	{
		name: 'pt-CH',
		englishName: 'Portuguese (Switzerland)',
	},
	{
		name: 'pt-TL',
		englishName: 'Portuguese (Timor-Leste)',
	},
	{
		name: 'pa',
		englishName: 'Punjabi',
	},
	{
		name: 'pa-Guru-IN',
		englishName: 'Punjabi (Gurmukhi, India)',
	},
	{
		name: 'pa-Guru',
		englishName: 'Punjabi (Gurmukhi)',
	},
	{
		name: 'pa-Arab-PK',
		englishName: 'Punjabi (Naskh, Pakistan)',
	},
	{
		name: 'pa-Arab',
		englishName: 'Punjabi (Naskh)',
	},
	{
		name: 'pa-Aran-PK',
		englishName: 'Punjabi (Nastaliq, Pakistan)',
	},
	{
		name: 'qu',
		englishName: 'Quechua',
	},
	{
		name: 'qu-BO',
		englishName: 'Quechua (Bolivia)',
	},
	{
		name: 'qu-EC',
		englishName: 'Quechua (Ecuador)',
	},
	{
		name: 'qu-PE',
		englishName: 'Quechua (Peru)',
	},
	{
		name: 'rhg',
		englishName: 'Rohingya',
	},
	{
		name: 'rhg-Rohg-BD',
		englishName: 'Rohingya (Hanifi, Bangladesh)',
	},
	{
		name: 'rhg-Rohg-MM',
		englishName: 'Rohingya (Hanifi, Myanmar [Burma])',
	},
	{
		name: 'rhg-Rohg',
		englishName: 'Rohingya (Hanifi)',
	},
	{
		name: 'ro',
		englishName: 'Romanian',
	},
	{
		name: 'ro-MD',
		englishName: 'Romanian (Moldova)',
	},
	{
		name: 'ro-RO',
		englishName: 'Romanian (Romania)',
	},
	{
		name: 'rm',
		englishName: 'Romansh',
	},
	{
		name: 'rm-CH',
		englishName: 'Romansh (Switzerland)',
	},
	{
		name: 'rof',
		englishName: 'Rombo',
	},
	{
		name: 'rof-TZ',
		englishName: 'Rombo (Tanzania)',
	},
	{
		name: 'rn',
		englishName: 'Rundi',
	},
	{
		name: 'rn-BI',
		englishName: 'Rundi (Burundi)',
	},
	{
		name: 'ru',
		englishName: 'Russian',
	},
	{
		name: 'ru-BY',
		englishName: 'Russian (Belarus)',
	},
	{
		name: 'ru-KZ',
		englishName: 'Russian (Kazakhstan)',
	},
	{
		name: 'ru-KG',
		englishName: 'Russian (Kyrgyzstan)',
	},
	{
		name: 'ru-MD',
		englishName: 'Russian (Moldova)',
	},
	{
		name: 'ru-RU',
		englishName: 'Russian (Russia)',
	},
	{
		name: 'ru-UA',
		englishName: 'Russian (Ukraine)',
	},
	{
		name: 'rwk',
		englishName: 'Rwa',
	},
	{
		name: 'rwk-TZ',
		englishName: 'Rwa (Tanzania)',
	},
	{
		name: 'sah',
		englishName: 'Sakha',
	},
	{
		name: 'sah-RU',
		englishName: 'Sakha (Russia)',
	},
	{
		name: 'saq',
		englishName: 'Samburu',
	},
	{
		name: 'saq-KE',
		englishName: 'Samburu (Kenya)',
	},
	{
		name: 'sm',
		englishName: 'Samoan',
	},
	{
		name: 'sm-AS',
		englishName: 'Samoan (American Samoa)',
	},
	{
		name: 'sm-WS',
		englishName: 'Samoan (Samoa)',
	},
	{
		name: 'sg',
		englishName: 'Sango',
	},
	{
		name: 'sg-CF',
		englishName: 'Sango (Central African Republic)',
	},
	{
		name: 'sbp',
		englishName: 'Sangu',
	},
	{
		name: 'sbp-TZ',
		englishName: 'Sangu (Tanzania)',
	},
	{
		name: 'sa',
		englishName: 'Sanskrit',
	},
	{
		name: 'sa-IN',
		englishName: 'Sanskrit (India)',
	},
	{
		name: 'sat',
		englishName: 'Santali',
	},
	{
		name: 'sat-Deva-IN',
		englishName: 'Santali (Devanagari, India)',
	},
	{
		name: 'sat-Deva',
		englishName: 'Santali (Devanagari)',
	},
	{
		name: 'sat-Olck-IN',
		englishName: 'Santali (Ol Chiki, India)',
	},
	{
		name: 'sat-Olck',
		englishName: 'Santali (Ol Chiki)',
	},
	{
		name: 'sc',
		englishName: 'Sardinian',
	},
	{
		name: 'sc-IT',
		englishName: 'Sardinian (Italy)',
	},
	{
		name: 'gd',
		englishName: 'Scottish Gaelic',
	},
	{
		name: 'gd-GB',
		englishName: 'Scottish Gaelic (United Kingdom)',
	},
	{
		name: 'seh',
		englishName: 'Sena',
	},
	{
		name: 'seh-MZ',
		englishName: 'Sena (Mozambique)',
	},
	{
		name: 'sr',
		englishName: 'Serbian',
	},
	{
		name: 'sr-Cyrl-BA',
		englishName: 'Serbian (Cyrillic, Bosnia & Herzegovina)',
	},
	{
		name: 'sr-Cyrl-XK',
		englishName: 'Serbian (Cyrillic, Kosovo)',
	},
	{
		name: 'sr-Cyrl-ME',
		englishName: 'Serbian (Cyrillic, Montenegro)',
	},
	{
		name: 'sr-Cyrl-RS',
		englishName: 'Serbian (Cyrillic, Serbia)',
	},
	{
		name: 'sr-Cyrl',
		englishName: 'Serbian (Cyrillic)',
	},
	{
		name: 'sr-Latn-BA',
		englishName: 'Serbian (Latin, Bosnia & Herzegovina)',
	},
	{
		name: 'sr-Latn-XK',
		englishName: 'Serbian (Latin, Kosovo)',
	},
	{
		name: 'sr-Latn-ME',
		englishName: 'Serbian (Latin, Montenegro)',
	},
	{
		name: 'sr-Latn-RS',
		englishName: 'Serbian (Latin, Serbia)',
	},
	{
		name: 'sr-Latn',
		englishName: 'Serbian (Latin)',
	},
	{
		name: 'ksb',
		englishName: 'Shambala',
	},
	{
		name: 'ksb-TZ',
		englishName: 'Shambala (Tanzania)',
	},
	{
		name: 'sn',
		englishName: 'Shona',
	},
	{
		name: 'sn-ZW',
		englishName: 'Shona (Zimbabwe)',
	},
	{
		name: 'ii',
		englishName: 'Sichuan Yi',
	},
	{
		name: 'ii-CN',
		englishName: 'Sichuan Yi (China mainland)',
	},
	{
		name: 'scn',
		englishName: 'Sicilian',
	},
	{
		name: 'scn-IT',
		englishName: 'Sicilian (Italy)',
	},
	{
		name: 'sd',
		englishName: 'Sindhi',
	},
	{
		name: 'sd-Arab-PK',
		englishName: 'Sindhi (Arabic, Pakistan)',
	},
	{
		name: 'sd-Arab',
		englishName: 'Sindhi (Arabic)',
	},
	{
		name: 'sd-Deva-IN',
		englishName: 'Sindhi (Devanagari, India)',
	},
	{
		name: 'sd-Deva',
		englishName: 'Sindhi (Devanagari)',
	},
	{
		name: 'si',
		englishName: 'Sinhala',
	},
	{
		name: 'si-LK',
		englishName: 'Sinhala (Sri Lanka)',
	},
	{
		name: 'sk',
		englishName: 'Slovak',
	},
	{
		name: 'sk-SK',
		englishName: 'Slovak (Slovakia)',
	},
	{
		name: 'sl',
		englishName: 'Slovenian',
	},
	{
		name: 'sl-SI',
		englishName: 'Slovenian (Slovenia)',
	},
	{
		name: 'xog',
		englishName: 'Soga',
	},
	{
		name: 'xog-UG',
		englishName: 'Soga (Uganda)',
	},
	{
		name: 'so',
		englishName: 'Somali',
	},
	{
		name: 'so-DJ',
		englishName: 'Somali (Djibouti)',
	},
	{
		name: 'so-ET',
		englishName: 'Somali (Ethiopia)',
	},
	{
		name: 'so-KE',
		englishName: 'Somali (Kenya)',
	},
	{
		name: 'so-SO',
		englishName: 'Somali (Somalia)',
	},
	{
		name: 'nr',
		englishName: 'South Ndebele',
	},
	{
		name: 'nr-ZA',
		englishName: 'South Ndebele (South Africa)',
	},
	{
		name: 'st',
		englishName: 'Southern Sotho',
	},
	{
		name: 'st-LS',
		englishName: 'Southern Sotho (Lesotho)',
	},
	{
		name: 'st-ZA',
		englishName: 'Southern Sotho (South Africa)',
	},
	{
		name: 'es',
		englishName: 'Spanish',
	},
	{
		name: 'es-AG',
		englishName: 'Spanish (Antigua & Barbuda)',
	},
	{
		name: 'es-AR',
		englishName: 'Spanish (Argentina)',
	},
	{
		name: 'es-BS',
		englishName: 'Spanish (Bahamas)',
	},
	{
		name: 'es-BB',
		englishName: 'Spanish (Barbados)',
	},
	{
		name: 'es-BZ',
		englishName: 'Spanish (Belize)',
	},
	{
		name: 'es-BM',
		englishName: 'Spanish (Bermuda)',
	},
	{
		name: 'es-BO',
		englishName: 'Spanish (Bolivia)',
	},
	{
		name: 'es-BR',
		englishName: 'Spanish (Brazil)',
	},
	{
		name: 'es-VG',
		englishName: 'Spanish (British Virgin Islands)',
	},
	{
		name: 'es-CA',
		englishName: 'Spanish (Canada)',
	},
	{
		name: 'es-IC',
		englishName: 'Spanish (Canary Islands)',
	},
	{
		name: 'es-BQ',
		englishName: 'Spanish (Caribbean Netherlands)',
	},
	{
		name: 'es-KY',
		englishName: 'Spanish (Cayman Islands)',
	},
	{
		name: 'es-EA',
		englishName: 'Spanish (Ceuta & Melilla)',
	},
	{
		name: 'es-CL',
		englishName: 'Spanish (Chile)',
	},
	{
		name: 'es-CO',
		englishName: 'Spanish (Colombia)',
	},
	{
		name: 'es-CR',
		englishName: 'Spanish (Costa Rica)',
	},
	{
		name: 'es-CU',
		englishName: 'Spanish (Cuba)',
	},
	{
		name: 'es-CW',
		englishName: 'Spanish (Curaçao)',
	},
	{
		name: 'es-DM',
		englishName: 'Spanish (Dominica)',
	},
	{
		name: 'es-DO',
		englishName: 'Spanish (Dominican Republic)',
	},
	{
		name: 'es-EC',
		englishName: 'Spanish (Ecuador)',
	},
	{
		name: 'es-SV',
		englishName: 'Spanish (El Salvador)',
	},
	{
		name: 'es-GQ',
		englishName: 'Spanish (Equatorial Guinea)',
	},
	{
		name: 'es-GD',
		englishName: 'Spanish (Grenada)',
	},
	{
		name: 'es-GT',
		englishName: 'Spanish (Guatemala)',
	},
	{
		name: 'es-GY',
		englishName: 'Spanish (Guyana)',
	},
	{
		name: 'es-HT',
		englishName: 'Spanish (Haiti)',
	},
	{
		name: 'es-HN',
		englishName: 'Spanish (Honduras)',
	},
	{
		name: 'es-419',
		englishName: 'Spanish (Latin America)',
	},
	{
		name: 'es-MX',
		englishName: 'Spanish (Mexico)',
	},
	{
		name: 'es-NI',
		englishName: 'Spanish (Nicaragua)',
	},
	{
		name: 'es-003',
		englishName: 'Spanish (North America)',
	},
	{
		name: 'es-PA',
		englishName: 'Spanish (Panama)',
	},
	{
		name: 'es-PY',
		englishName: 'Spanish (Paraguay)',
	},
	{
		name: 'es-PE',
		englishName: 'Spanish (Peru)',
	},
	{
		name: 'es-PH',
		englishName: 'Spanish (Philippines)',
	},
	{
		name: 'es-PR',
		englishName: 'Spanish (Puerto Rico)',
	},
	{
		name: 'es-ES',
		englishName: 'Spanish (Spain)',
	},
	{
		name: 'es-KN',
		englishName: 'Spanish (St. Kitts & Nevis)',
	},
	{
		name: 'es-LC',
		englishName: 'Spanish (St. Lucia)',
	},
	{
		name: 'es-VC',
		englishName: 'Spanish (St. Vincent & Grenadines)',
	},
	{
		name: 'es-TT',
		englishName: 'Spanish (Trinidad & Tobago)',
	},
	{
		name: 'es-TC',
		englishName: 'Spanish (Turks & Caicos Islands)',
	},
	{
		name: 'es-VI',
		englishName: 'Spanish (U.S. Virgin Islands)',
	},
	{
		name: 'es-US',
		englishName: 'Spanish (United States)',
	},
	{
		name: 'es-UY',
		englishName: 'Spanish (Uruguay)',
	},
	{
		name: 'es-VE',
		englishName: 'Spanish (Venezuela)',
	},
	{
		name: 'zgh',
		englishName: 'Standard Moroccan Tamazight',
	},
	{
		name: 'zgh-MA',
		englishName: 'Standard Moroccan Tamazight (Morocco)',
	},
	{
		name: 'su',
		englishName: 'Sundanese',
	},
	{
		name: 'su-Latn-ID',
		englishName: 'Sundanese (Latin, Indonesia)',
	},
	{
		name: 'su-Latn',
		englishName: 'Sundanese (Latin)',
	},
	{
		name: 'sw',
		englishName: 'Swahili',
	},
	{
		name: 'sw-CD',
		englishName: 'Swahili (Congo - Kinshasa)',
	},
	{
		name: 'sw-KE',
		englishName: 'Swahili (Kenya)',
	},
	{
		name: 'sw-TZ',
		englishName: 'Swahili (Tanzania)',
	},
	{
		name: 'sw-UG',
		englishName: 'Swahili (Uganda)',
	},
	{
		name: 'ss',
		englishName: 'Swati',
	},
	{
		name: 'ss-SZ',
		englishName: 'Swati (Eswatini)',
	},
	{
		name: 'ss-ZA',
		englishName: 'Swati (South Africa)',
	},
	{
		name: 'sv',
		englishName: 'Swedish',
	},
	{
		name: 'sv-AX',
		englishName: 'Swedish (Åland Islands)',
	},
	{
		name: 'sv-FI',
		englishName: 'Swedish (Finland)',
	},
	{
		name: 'sv-SE',
		englishName: 'Swedish (Sweden)',
	},
	{
		name: 'gsw',
		englishName: 'Swiss German',
	},
	{
		name: 'gsw-FR',
		englishName: 'Swiss German (France)',
	},
	{
		name: 'gsw-LI',
		englishName: 'Swiss German (Liechtenstein)',
	},
	{
		name: 'gsw-CH',
		englishName: 'Swiss German (Switzerland)',
	},
	{
		name: 'shi',
		englishName: 'Tachelhit',
	},
	{
		name: 'shi-Latn-MA',
		englishName: 'Tachelhit (Latin, Morocco)',
	},
	{
		name: 'shi-Latn',
		englishName: 'Tachelhit (Latin)',
	},
	{
		name: 'shi-Tfng-MA',
		englishName: 'Tachelhit (Tifinagh, Morocco)',
	},
	{
		name: 'shi-Tfng',
		englishName: 'Tachelhit (Tifinagh)',
	},
	{
		name: 'dav',
		englishName: 'Taita',
	},
	{
		name: 'dav-KE',
		englishName: 'Taita (Kenya)',
	},
	{
		name: 'tg',
		englishName: 'Tajik',
	},
	{
		name: 'tg-TJ',
		englishName: 'Tajik (Tajikistan)',
	},
	{
		name: 'ta',
		englishName: 'Tamil',
	},
	{
		name: 'ta-IN',
		englishName: 'Tamil (India)',
	},
	{
		name: 'ta-MY',
		englishName: 'Tamil (Malaysia)',
	},
	{
		name: 'ta-SG',
		englishName: 'Tamil (Singapore)',
	},
	{
		name: 'ta-LK',
		englishName: 'Tamil (Sri Lanka)',
	},
	{
		name: 'trv',
		englishName: 'Taroko',
	},
	{
		name: 'trv-TW',
		englishName: 'Taroko (Taiwan)',
	},
	{
		name: 'twq',
		englishName: 'Tasawaq',
	},
	{
		name: 'twq-NE',
		englishName: 'Tasawaq (Niger)',
	},
	{
		name: 'tt',
		englishName: 'Tatar',
	},
	{
		name: 'tt-RU',
		englishName: 'Tatar (Russia)',
	},
	{
		name: 'te',
		englishName: 'Telugu',
	},
	{
		name: 'te-IN',
		englishName: 'Telugu (India)',
	},
	{
		name: 'teo',
		englishName: 'Teso',
	},
	{
		name: 'teo-KE',
		englishName: 'Teso (Kenya)',
	},
	{
		name: 'teo-UG',
		englishName: 'Teso (Uganda)',
	},
	{
		name: 'th',
		englishName: 'Thai',
	},
	{
		name: 'th-TH',
		englishName: 'Thai (Thailand)',
	},
	{
		name: 'bo',
		englishName: 'Tibetan',
	},
	{
		name: 'bo-CN',
		englishName: 'Tibetan (China mainland)',
	},
	{
		name: 'bo-IN',
		englishName: 'Tibetan (India)',
	},
	{
		name: 'tig',
		englishName: 'Tigre',
	},
	{
		name: 'tig-ER',
		englishName: 'Tigre (Eritrea)',
	},
	{
		name: 'ti',
		englishName: 'Tigrinya',
	},
	{
		name: 'ti-ER',
		englishName: 'Tigrinya (Eritrea)',
	},
	{
		name: 'ti-ET',
		englishName: 'Tigrinya (Ethiopia)',
	},
	{
		name: 'to',
		englishName: 'Tongan',
	},
	{
		name: 'to-TO',
		englishName: 'Tongan (Tonga)',
	},
	{
		name: 'ts',
		englishName: 'Tsonga',
	},
	{
		name: 'ts-ZA',
		englishName: 'Tsonga (South Africa)',
	},
	{
		name: 'tn',
		englishName: 'Tswana',
	},
	{
		name: 'tn-BW',
		englishName: 'Tswana (Botswana)',
	},
	{
		name: 'tn-ZA',
		englishName: 'Tswana (South Africa)',
	},
	{
		name: 'tr',
		englishName: 'Turkish',
	},
	{
		name: 'tr-CY',
		englishName: 'Turkish (Cyprus)',
	},
	{
		name: 'tr-TR',
		englishName: 'Turkish (Turkey)',
	},
	{
		name: 'tk',
		englishName: 'Turkmen',
	},
	{
		name: 'tk-TM',
		englishName: 'Turkmen (Turkmenistan)',
	},
	{
		name: 'kcg',
		englishName: 'Tyap',
	},
	{
		name: 'kcg-NG',
		englishName: 'Tyap (Nigeria)',
	},
	{
		name: 'uk',
		englishName: 'Ukrainian',
	},
	{
		name: 'uk-UA',
		englishName: 'Ukrainian (Ukraine)',
	},
	{
		name: 'hsb',
		englishName: 'Upper Sorbian',
	},
	{
		name: 'hsb-DE',
		englishName: 'Upper Sorbian (Germany)',
	},
	{
		name: 'ur',
		englishName: 'Urdu',
	},
	{
		name: 'ur-Arab-IN',
		englishName: 'Urdu (Naskh, India)',
	},
	{
		name: 'ur-Arab-PK',
		englishName: 'Urdu (Naskh, Pakistan)',
	},
	{
		name: 'ur-Arab',
		englishName: 'Urdu (Naskh)',
	},
	{
		name: 'ur-Aran-PK',
		englishName: 'Urdu (Nastaliq, Pakistan)',
	},
	{
		name: 'ug',
		englishName: 'Uyghur',
	},
	{
		name: 'ug-CN',
		englishName: 'Uyghur (China mainland)',
	},
	{
		name: 'uz',
		englishName: 'Uzbek',
	},
	{
		name: 'uz-Arab-AF',
		englishName: 'Uzbek (Arabic, Afghanistan)',
	},
	{
		name: 'uz-Arab',
		englishName: 'Uzbek (Arabic)',
	},
	{
		name: 'uz-Cyrl-UZ',
		englishName: 'Uzbek (Cyrillic, Uzbekistan)',
	},
	{
		name: 'uz-Cyrl',
		englishName: 'Uzbek (Cyrillic)',
	},
	{
		name: 'uz-Latn-UZ',
		englishName: 'Uzbek (Latin, Uzbekistan)',
	},
	{
		name: 'uz-Latn',
		englishName: 'Uzbek (Latin)',
	},
	{
		name: 'vai',
		englishName: 'Vai',
	},
	{
		name: 'vai-Latn-LR',
		englishName: 'Vai (Latin, Liberia)',
	},
	{
		name: 'vai-Latn',
		englishName: 'Vai (Latin)',
	},
	{
		name: 'vai-Vaii-LR',
		englishName: 'Vai (Vai, Liberia)',
	},
	{
		name: 'vai-Vaii',
		englishName: 'Vai (Vai)',
	},
	{
		name: 've',
		englishName: 'Venda',
	},
	{
		name: 've-ZA',
		englishName: 'Venda (South Africa)',
	},
	{
		name: 'vi',
		englishName: 'Vietnamese',
	},
	{
		name: 'vi-VN',
		englishName: 'Vietnamese (Vietnam)',
	},
	{
		name: 'vun',
		englishName: 'Vunjo',
	},
	{
		name: 'vun-TZ',
		englishName: 'Vunjo (Tanzania)',
	},
	{
		name: 'wa',
		englishName: 'Walloon',
	},
	{
		name: 'wa-BE',
		englishName: 'Walloon (Belgium)',
	},
	{
		name: 'wae',
		englishName: 'Walser',
	},
	{
		name: 'wae-CH',
		englishName: 'Walser (Switzerland)',
	},
	{
		name: 'cy',
		englishName: 'Welsh',
	},
	{
		name: 'cy-GB',
		englishName: 'Welsh (United Kingdom)',
	},
	{
		name: 'fy',
		englishName: 'Western Frisian',
	},
	{
		name: 'fy-NL',
		englishName: 'Western Frisian (Netherlands)',
	},
	{
		name: 'wal',
		englishName: 'Wolaytta',
	},
	{
		name: 'wal-ET',
		englishName: 'Wolaytta (Ethiopia)',
	},
	{
		name: 'wo',
		englishName: 'Wolof',
	},
	{
		name: 'wo-SN',
		englishName: 'Wolof (Senegal)',
	},
	{
		name: 'xh',
		englishName: 'Xhosa',
	},
	{
		name: 'xh-ZA',
		englishName: 'Xhosa (South Africa)',
	},
	{
		name: 'yav',
		englishName: 'Yangben',
	},
	{
		name: 'yav-CM',
		englishName: 'Yangben (Cameroon)',
	},
	{
		name: 'yi',
		englishName: 'Yiddish',
	},
	{
		name: 'yi-001',
		englishName: 'Yiddish (world)',
	},
	{
		name: 'yo',
		englishName: 'Yoruba',
	},
	{
		name: 'yo-BJ',
		englishName: 'Yoruba (Benin)',
	},
	{
		name: 'yo-NG',
		englishName: 'Yoruba (Nigeria)',
	},
	{
		name: 'dje',
		englishName: 'Zarma',
	},
	{
		name: 'dje-NE',
		englishName: 'Zarma (Niger)',
	},
	{
		name: 'zu',
		englishName: 'Zulu',
	},
	{
		name: 'zu-ZA',
		englishName: 'Zulu (South Africa)',
	},
];

export const umbCulturesData = new UmbCulturesData();
