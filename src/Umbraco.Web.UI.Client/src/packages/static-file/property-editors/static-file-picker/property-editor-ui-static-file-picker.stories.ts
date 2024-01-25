import type { Meta, Story } from '@storybook/web-components';
import type { UmbPropertyEditorUIStaticFilePickerElement } from './property-editor-ui-static-file-picker.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-static-file-picker.element.js';

export default {
	title: 'Property Editor UIs/Static File Picker',
	component: 'umb-property-editor-ui-static-file-picker',
	id: 'umb-property-editor-ui-static-file-picker',
} as Meta;

export const AAAOverview: Story<UmbPropertyEditorUIStaticFilePickerElement> = () =>
	html` <umb-property-editor-ui-static-file-picker></umb-property-editor-ui-static-file-picker>`;
AAAOverview.storyName = 'Overview';
