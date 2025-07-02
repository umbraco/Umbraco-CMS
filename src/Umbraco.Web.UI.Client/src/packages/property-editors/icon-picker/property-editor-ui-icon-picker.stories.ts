import type { UmbIconPickerModalElement } from '@umbraco-cms/backoffice/icon';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-icon-picker.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Icon Picker',
	component: 'umb-property-editor-ui-icon-picker',
	id: 'umb-property-editor-ui-icon-picker',
} as Meta;

export const AAAOverview: StoryFn<UmbIconPickerModalElement> = () =>
	html`<umb-icon-picker-modal></umb-icon-picker-modal>`;
AAAOverview.storyName = 'Overview';
AAAOverview.decorators = [
	(story) =>
		html`<div style="width:400px; position:absolute; margin: 20px 0;top:0; bottom:0; border:1px solid #ccc">
			${story()}
		</div>`,
];
