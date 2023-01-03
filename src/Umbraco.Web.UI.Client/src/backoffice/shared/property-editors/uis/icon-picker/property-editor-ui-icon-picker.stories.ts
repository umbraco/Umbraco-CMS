import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbPropertyEditorUIIconPickerElement } from './property-editor-ui-icon-picker.element';
import './property-editor-ui-icon-picker.element';
import type { UmbModalLayoutIconPickerElement } from 'src/core/modal/layouts/icon-picker/modal-layout-icon-picker.element';

export default {
	title: 'Property Editor UIs/Icon Picker',
	component: 'umb-property-editor-ui-icon-picker',
	id: 'umb-property-editor-ui-icon-picker',
} as Meta;

export const AAAOverview: Story<UmbModalLayoutIconPickerElement> = () =>
	html`<umb-modal-layout-icon-picker></umb-modal-layout-icon-picker>`;
AAAOverview.storyName = 'Overview';
AAAOverview.decorators = [
	(story) =>
		html`<div style="width:400px; position:absolute; margin: 20px 0;top:0; bottom:0; border:1px solid #ccc">
			${story()}
		</div>`,
];
