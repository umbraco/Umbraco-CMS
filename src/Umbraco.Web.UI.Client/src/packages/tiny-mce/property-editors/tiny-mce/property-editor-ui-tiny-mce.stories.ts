import type { UmbPropertyEditorUITinyMceElement } from './property-editor-ui-tiny-mce.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

import './property-editor-ui-tiny-mce.element.js';

const config = new UmbPropertyEditorConfigCollection([
	{
		alias: 'hideLabel',
		value: true,
	},
	{ alias: 'dimensions', value: { height: 500 } },
	{ alias: 'maxImageSize', value: 500 },
	{ alias: 'ignoreUserStartNodes', value: false },
	{
		alias: 'validElements',
		value:
			'+a[id|style|rel|data-id|data-udi|rev|charset|hreflang|dir|lang|tabindex|accesskey|type|name|href|target|title|class|onfocus|onblur|onclick|ondblclick|onmousedown|onmouseup|onmouseover|onmousemove|onmouseout|onkeypress|onkeydown|onkeyup],-strong/-b[class|style],-em/-i[class|style],-strike[class|style],-s[class|style],-u[class|style],#p[id|style|dir|class|align],-ol[class|reversed|start|style|type],-ul[class|style],-li[class|style],br[class],img[id|dir|lang|longdesc|usemap|style|class|src|onmouseover|onmouseout|border|alt=|title|hspace|vspace|width|height|align|umbracoorgwidth|umbracoorgheight|onresize|onresizestart|onresizeend|rel|data-id],-sub[style|class],-sup[style|class],-blockquote[dir|style|class],-table[border=0|cellspacing|cellpadding|width|height|class|align|summary|style|dir|id|lang|bgcolor|background|bordercolor],-tr[id|lang|dir|class|rowspan|width|height|align|valign|style|bgcolor|background|bordercolor],tbody[id|class],thead[id|class],tfoot[id|class],#td[id|lang|dir|class|colspan|rowspan|width|height|align|valign|style|bgcolor|background|bordercolor|scope],-th[id|lang|dir|class|colspan|rowspan|width|height|align|valign|style|scope],caption[id|lang|dir|class|style],-div[id|dir|class|align|style],-span[class|align|style],-pre[class|align|style],address[class|align|style],-h1[id|dir|class|align|style],-h2[id|dir|class|align|style],-h3[id|dir|class|align|style],-h4[id|dir|class|align|style],-h5[id|dir|class|align|style],-h6[id|style|dir|class|align|style],hr[class|style],small[class|style],dd[id|class|title|style|dir|lang],dl[id|class|title|style|dir|lang],dt[id|class|title|style|dir|lang],object[class|id|width|height|codebase|*],param[name|value|_value|class],embed[type|width|height|src|class|*],map[name|class],area[shape|coords|href|alt|target|class],bdo[class],button[class],iframe[*],figure,figcaption,video[*],audio[*],picture[*],source[*],canvas[*]',
	},
	{ alias: 'invalidElements', value: 'font' },
	{
		alias: 'toolbar',
		value: [
			'sourcecode',
			'undo',
			'redo',
			'styles',
			'bold',
			'italic',
			'alignleft',
			'aligncenter',
			'alignright',
			'bullist',
			'numlist',
			'outdent',
			'indent',
			'link',
			'unlink',
			'anchor',
			'table',
			'umbmediapicker',
			'umbembeddialog',
		],
	},
	{
		alias: 'plugins',
		value: [
			{
				name: 'anchor',
			},
			{
				name: 'charmap',
			},
			{
				name: 'table',
			},
			{
				name: 'lists',
			},
			{
				name: 'advlist',
			},
			{
				name: 'autolink',
			},
			{
				name: 'directionality',
			},
			{
				name: 'searchreplace',
			},
		],
	},
]);

const meta: Meta<UmbPropertyEditorUITinyMceElement> = {
	title: 'Property Editor UIs/Tiny Mce',
	component: 'umb-property-editor-ui-tiny-mce',
	id: 'umb-property-editor-ui-tiny-mce',
	args: {
		config: undefined,
		value: {
			blocks: {
				layout: {},
				contentData: [],
				settingsData: [],
				expose: [],
			},
			markup: `
			<h2>TinyMCE</h2>
			<p>I am a default value for the TinyMCE text editor story.</p>
			<p>
				<a href="https://www.tiny.cloud/docs/" target="_blank" rel="noopener noreferrer">TinyMCE documentation</a>
			</p>
			<p>
				<a href="https://www.tiny.cloud/docs/quick-start/" target="_blank" rel="noopener noreferrer">TinyMCE quick start guide</a>
			</p>
				<a href="https://docs.umbraco.com" target="_blank" rel="noopener noreferrer">Umbraco documentation</a>
			</p>
		`,
		},
	},
};

export default meta;
type Story = StoryObj<UmbPropertyEditorUITinyMceElement>;

export const Default: Story = {};

export const DefaultConfig: Story = {
	args: {
		config,
	},
};
