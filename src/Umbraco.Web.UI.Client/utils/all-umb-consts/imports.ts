
		import * as import0 from '@umbraco-cms/backoffice/app';
import * as import1 from '@umbraco-cms/backoffice/class-api';
import * as import2 from '@umbraco-cms/backoffice/context-api';
import * as import3 from '@umbraco-cms/backoffice/controller-api';
import * as import4 from '@umbraco-cms/backoffice/element-api';
import * as import5 from '@umbraco-cms/backoffice/embedded-media';
import * as import6 from '@umbraco-cms/backoffice/extension-api';
import * as import7 from '@umbraco-cms/backoffice/localization-api';
import * as import8 from '@umbraco-cms/backoffice/observable-api';
import * as import9 from '@umbraco-cms/backoffice/action';
import * as import10 from '@umbraco-cms/backoffice/audit-log';
import * as import11 from '@umbraco-cms/backoffice/auth';
import * as import12 from '@umbraco-cms/backoffice/block-custom-view';
import * as import13 from '@umbraco-cms/backoffice/block-grid';
import * as import14 from '@umbraco-cms/backoffice/block-list';
import * as import15 from '@umbraco-cms/backoffice/block-rte';
import * as import16 from '@umbraco-cms/backoffice/block-type';
import * as import17 from '@umbraco-cms/backoffice/block';
import * as import18 from '@umbraco-cms/backoffice/clipboard';
import * as import19 from '@umbraco-cms/backoffice/code-editor';
import * as import20 from '@umbraco-cms/backoffice/collection';
import * as import21 from '@umbraco-cms/backoffice/components';
import * as import22 from '@umbraco-cms/backoffice/const';
import * as import23 from '@umbraco-cms/backoffice/content-type';
import * as import24 from '@umbraco-cms/backoffice/content';
import * as import25 from '@umbraco-cms/backoffice/culture';
import * as import26 from '@umbraco-cms/backoffice/current-user';
import * as import27 from '@umbraco-cms/backoffice/dashboard';
import * as import28 from '@umbraco-cms/backoffice/data-type';
import * as import29 from '@umbraco-cms/backoffice/debug';
import * as import30 from '@umbraco-cms/backoffice/dictionary';
import * as import31 from '@umbraco-cms/backoffice/document-blueprint';
import * as import32 from '@umbraco-cms/backoffice/document-type';
import * as import33 from '@umbraco-cms/backoffice/document';
import * as import34 from '@umbraco-cms/backoffice/entity-action';
import * as import35 from '@umbraco-cms/backoffice/entity-bulk-action';
import * as import36 from '@umbraco-cms/backoffice/entity-create-option-action';
import * as import37 from '@umbraco-cms/backoffice/entity';
import * as import38 from '@umbraco-cms/backoffice/entity-item';
import * as import39 from '@umbraco-cms/backoffice/event';
import * as import40 from '@umbraco-cms/backoffice/extension-registry';
import * as import41 from '@umbraco-cms/backoffice/health-check';
import * as import42 from '@umbraco-cms/backoffice/help';
import * as import43 from '@umbraco-cms/backoffice/icon';
import * as import44 from '@umbraco-cms/backoffice/id';
import * as import45 from '@umbraco-cms/backoffice/imaging';
import * as import46 from '@umbraco-cms/backoffice/language';
import * as import47 from '@umbraco-cms/backoffice/lit-element';
import * as import48 from '@umbraco-cms/backoffice/localization';
import * as import49 from '@umbraco-cms/backoffice/log-viewer';
import * as import50 from '@umbraco-cms/backoffice/media-type';
import * as import51 from '@umbraco-cms/backoffice/media';
import * as import52 from '@umbraco-cms/backoffice/member-group';
import * as import53 from '@umbraco-cms/backoffice/member-type';
import * as import54 from '@umbraco-cms/backoffice/member';
import * as import55 from '@umbraco-cms/backoffice/member-public-access';
import * as import56 from '@umbraco-cms/backoffice/menu';
import * as import57 from '@umbraco-cms/backoffice/modal';
import * as import58 from '@umbraco-cms/backoffice/multi-url-picker';
import * as import59 from '@umbraco-cms/backoffice/notification';
import * as import60 from '@umbraco-cms/backoffice/object-type';
import * as import61 from '@umbraco-cms/backoffice/package';
import * as import62 from '@umbraco-cms/backoffice/partial-view';
import * as import63 from '@umbraco-cms/backoffice/picker-input';
import * as import64 from '@umbraco-cms/backoffice/picker';
import * as import65 from '@umbraco-cms/backoffice/property-action';
import * as import66 from '@umbraco-cms/backoffice/property-editor';
import * as import67 from '@umbraco-cms/backoffice/property-type';
import * as import68 from '@umbraco-cms/backoffice/property';
import * as import69 from '@umbraco-cms/backoffice/recycle-bin';
import * as import70 from '@umbraco-cms/backoffice/relation-type';
import * as import71 from '@umbraco-cms/backoffice/relations';
import * as import72 from '@umbraco-cms/backoffice/repository';
import * as import73 from '@umbraco-cms/backoffice/resources';
import * as import74 from '@umbraco-cms/backoffice/router';
import * as import75 from '@umbraco-cms/backoffice/rte';
import * as import76 from '@umbraco-cms/backoffice/script';
import * as import77 from '@umbraco-cms/backoffice/search';
import * as import78 from '@umbraco-cms/backoffice/section';
import * as import79 from '@umbraco-cms/backoffice/server-file-system';
import * as import80 from '@umbraco-cms/backoffice/settings';
import * as import81 from '@umbraco-cms/backoffice/sorter';
import * as import82 from '@umbraco-cms/backoffice/static-file';
import * as import83 from '@umbraco-cms/backoffice/store';
import * as import84 from '@umbraco-cms/backoffice/style';
import * as import85 from '@umbraco-cms/backoffice/stylesheet';
import * as import86 from '@umbraco-cms/backoffice/sysinfo';
import * as import87 from '@umbraco-cms/backoffice/tags';
import * as import88 from '@umbraco-cms/backoffice/template';
import * as import89 from '@umbraco-cms/backoffice/temporary-file';
import * as import90 from '@umbraco-cms/backoffice/themes';
import * as import91 from '@umbraco-cms/backoffice/tiny-mce';
import * as import92 from '@umbraco-cms/backoffice/tiptap';
import * as import93 from '@umbraco-cms/backoffice/translation';
import * as import94 from '@umbraco-cms/backoffice/tree';
import * as import95 from '@umbraco-cms/backoffice/ufm';
import * as import96 from '@umbraco-cms/backoffice/user-change-password';
import * as import97 from '@umbraco-cms/backoffice/user-group';
import * as import98 from '@umbraco-cms/backoffice/user-permission';
import * as import99 from '@umbraco-cms/backoffice/user';
import * as import100 from '@umbraco-cms/backoffice/utils';
import * as import101 from '@umbraco-cms/backoffice/validation';
import * as import102 from '@umbraco-cms/backoffice/variant';
import * as import103 from '@umbraco-cms/backoffice/webhook';
import * as import104 from '@umbraco-cms/backoffice/workspace';

		export const imports = [
			{
			path: '@umbraco-cms/backoffice/app',
			package: import0
		},
{
			path: '@umbraco-cms/backoffice/class-api',
			package: import1
		},
{
			path: '@umbraco-cms/backoffice/context-api',
			package: import2
		},
{
			path: '@umbraco-cms/backoffice/controller-api',
			package: import3
		},
{
			path: '@umbraco-cms/backoffice/element-api',
			package: import4
		},
{
			path: '@umbraco-cms/backoffice/embedded-media',
			package: import5
		},
{
			path: '@umbraco-cms/backoffice/extension-api',
			package: import6
		},
{
			path: '@umbraco-cms/backoffice/localization-api',
			package: import7
		},
{
			path: '@umbraco-cms/backoffice/observable-api',
			package: import8
		},
{
			path: '@umbraco-cms/backoffice/action',
			package: import9
		},
{
			path: '@umbraco-cms/backoffice/audit-log',
			package: import10
		},
{
			path: '@umbraco-cms/backoffice/auth',
			package: import11
		},
{
			path: '@umbraco-cms/backoffice/block-custom-view',
			package: import12
		},
{
			path: '@umbraco-cms/backoffice/block-grid',
			package: import13
		},
{
			path: '@umbraco-cms/backoffice/block-list',
			package: import14
		},
{
			path: '@umbraco-cms/backoffice/block-rte',
			package: import15
		},
{
			path: '@umbraco-cms/backoffice/block-type',
			package: import16
		},
{
			path: '@umbraco-cms/backoffice/block',
			package: import17
		},
{
			path: '@umbraco-cms/backoffice/clipboard',
			package: import18
		},
{
			path: '@umbraco-cms/backoffice/code-editor',
			package: import19
		},
{
			path: '@umbraco-cms/backoffice/collection',
			package: import20
		},
{
			path: '@umbraco-cms/backoffice/components',
			package: import21
		},
{
			path: '@umbraco-cms/backoffice/const',
			package: import22
		},
{
			path: '@umbraco-cms/backoffice/content-type',
			package: import23
		},
{
			path: '@umbraco-cms/backoffice/content',
			package: import24
		},
{
			path: '@umbraco-cms/backoffice/culture',
			package: import25
		},
{
			path: '@umbraco-cms/backoffice/current-user',
			package: import26
		},
{
			path: '@umbraco-cms/backoffice/dashboard',
			package: import27
		},
{
			path: '@umbraco-cms/backoffice/data-type',
			package: import28
		},
{
			path: '@umbraco-cms/backoffice/debug',
			package: import29
		},
{
			path: '@umbraco-cms/backoffice/dictionary',
			package: import30
		},
{
			path: '@umbraco-cms/backoffice/document-blueprint',
			package: import31
		},
{
			path: '@umbraco-cms/backoffice/document-type',
			package: import32
		},
{
			path: '@umbraco-cms/backoffice/document',
			package: import33
		},
{
			path: '@umbraco-cms/backoffice/entity-action',
			package: import34
		},
{
			path: '@umbraco-cms/backoffice/entity-bulk-action',
			package: import35
		},
{
			path: '@umbraco-cms/backoffice/entity-create-option-action',
			package: import36
		},
{
			path: '@umbraco-cms/backoffice/entity',
			package: import37
		},
{
			path: '@umbraco-cms/backoffice/entity-item',
			package: import38
		},
{
			path: '@umbraco-cms/backoffice/event',
			package: import39
		},
{
			path: '@umbraco-cms/backoffice/extension-registry',
			package: import40
		},
{
			path: '@umbraco-cms/backoffice/health-check',
			package: import41
		},
{
			path: '@umbraco-cms/backoffice/help',
			package: import42
		},
{
			path: '@umbraco-cms/backoffice/icon',
			package: import43
		},
{
			path: '@umbraco-cms/backoffice/id',
			package: import44
		},
{
			path: '@umbraco-cms/backoffice/imaging',
			package: import45
		},
{
			path: '@umbraco-cms/backoffice/language',
			package: import46
		},
{
			path: '@umbraco-cms/backoffice/lit-element',
			package: import47
		},
{
			path: '@umbraco-cms/backoffice/localization',
			package: import48
		},
{
			path: '@umbraco-cms/backoffice/log-viewer',
			package: import49
		},
{
			path: '@umbraco-cms/backoffice/media-type',
			package: import50
		},
{
			path: '@umbraco-cms/backoffice/media',
			package: import51
		},
{
			path: '@umbraco-cms/backoffice/member-group',
			package: import52
		},
{
			path: '@umbraco-cms/backoffice/member-type',
			package: import53
		},
{
			path: '@umbraco-cms/backoffice/member',
			package: import54
		},
{
			path: '@umbraco-cms/backoffice/member-public-access',
			package: import55
		},
{
			path: '@umbraco-cms/backoffice/menu',
			package: import56
		},
{
			path: '@umbraco-cms/backoffice/modal',
			package: import57
		},
{
			path: '@umbraco-cms/backoffice/multi-url-picker',
			package: import58
		},
{
			path: '@umbraco-cms/backoffice/notification',
			package: import59
		},
{
			path: '@umbraco-cms/backoffice/object-type',
			package: import60
		},
{
			path: '@umbraco-cms/backoffice/package',
			package: import61
		},
{
			path: '@umbraco-cms/backoffice/partial-view',
			package: import62
		},
{
			path: '@umbraco-cms/backoffice/picker-input',
			package: import63
		},
{
			path: '@umbraco-cms/backoffice/picker',
			package: import64
		},
{
			path: '@umbraco-cms/backoffice/property-action',
			package: import65
		},
{
			path: '@umbraco-cms/backoffice/property-editor',
			package: import66
		},
{
			path: '@umbraco-cms/backoffice/property-type',
			package: import67
		},
{
			path: '@umbraco-cms/backoffice/property',
			package: import68
		},
{
			path: '@umbraco-cms/backoffice/recycle-bin',
			package: import69
		},
{
			path: '@umbraco-cms/backoffice/relation-type',
			package: import70
		},
{
			path: '@umbraco-cms/backoffice/relations',
			package: import71
		},
{
			path: '@umbraco-cms/backoffice/repository',
			package: import72
		},
{
			path: '@umbraco-cms/backoffice/resources',
			package: import73
		},
{
			path: '@umbraco-cms/backoffice/router',
			package: import74
		},
{
			path: '@umbraco-cms/backoffice/rte',
			package: import75
		},
{
			path: '@umbraco-cms/backoffice/script',
			package: import76
		},
{
			path: '@umbraco-cms/backoffice/search',
			package: import77
		},
{
			path: '@umbraco-cms/backoffice/section',
			package: import78
		},
{
			path: '@umbraco-cms/backoffice/server-file-system',
			package: import79
		},
{
			path: '@umbraco-cms/backoffice/settings',
			package: import80
		},
{
			path: '@umbraco-cms/backoffice/sorter',
			package: import81
		},
{
			path: '@umbraco-cms/backoffice/static-file',
			package: import82
		},
{
			path: '@umbraco-cms/backoffice/store',
			package: import83
		},
{
			path: '@umbraco-cms/backoffice/style',
			package: import84
		},
{
			path: '@umbraco-cms/backoffice/stylesheet',
			package: import85
		},
{
			path: '@umbraco-cms/backoffice/sysinfo',
			package: import86
		},
{
			path: '@umbraco-cms/backoffice/tags',
			package: import87
		},
{
			path: '@umbraco-cms/backoffice/template',
			package: import88
		},
{
			path: '@umbraco-cms/backoffice/temporary-file',
			package: import89
		},
{
			path: '@umbraco-cms/backoffice/themes',
			package: import90
		},
{
			path: '@umbraco-cms/backoffice/tiny-mce',
			package: import91
		},
{
			path: '@umbraco-cms/backoffice/tiptap',
			package: import92
		},
{
			path: '@umbraco-cms/backoffice/translation',
			package: import93
		},
{
			path: '@umbraco-cms/backoffice/tree',
			package: import94
		},
{
			path: '@umbraco-cms/backoffice/ufm',
			package: import95
		},
{
			path: '@umbraco-cms/backoffice/user-change-password',
			package: import96
		},
{
			path: '@umbraco-cms/backoffice/user-group',
			package: import97
		},
{
			path: '@umbraco-cms/backoffice/user-permission',
			package: import98
		},
{
			path: '@umbraco-cms/backoffice/user',
			package: import99
		},
{
			path: '@umbraco-cms/backoffice/utils',
			package: import100
		},
{
			path: '@umbraco-cms/backoffice/validation',
			package: import101
		},
{
			path: '@umbraco-cms/backoffice/variant',
			package: import102
		},
{
			path: '@umbraco-cms/backoffice/webhook',
			package: import103
		},
{
			path: '@umbraco-cms/backoffice/workspace',
			package: import104
		}
		];
	