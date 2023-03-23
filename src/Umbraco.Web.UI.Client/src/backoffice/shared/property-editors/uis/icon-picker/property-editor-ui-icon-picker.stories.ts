import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbIconPickerModalElement } from '../../../../shared/modals/icon-picker/icon-picker-modal.element';
import type { UmbPropertyEditorUIIconPickerElement } from './property-editor-ui-icon-picker.element';
import './property-editor-ui-icon-picker.element';

export default {
	title: 'Property Editor UIs/Icon Picker',
	component: 'umb-property-editor-ui-icon-picker',
	id: 'umb-property-editor-ui-icon-picker',
} as Meta;

export const AAAOverview: Story<UmbIconPickerModalElement> = () =>
	html`<umb-icon-picker-modal></umb-icon-picker-modal>`;
AAAOverview.storyName = 'Overview';
AAAOverview.decorators = [
	(story) =>
		html`<div style="width:400px; position:absolute; margin: 20px 0;top:0; bottom:0; border:1px solid #ccc">
			${story()}
		</div>`,
];
