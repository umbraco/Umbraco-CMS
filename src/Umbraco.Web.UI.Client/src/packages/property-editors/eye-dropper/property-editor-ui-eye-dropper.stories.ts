import type { UmbPropertyEditorUIEyeDropperElement } from './property-editor-ui-eye-dropper.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-eye-dropper.element.js';

export default {
	title: 'Property Editor UIs/Eye Dropper',
	component: 'umb-property-editor-ui-eye-dropper',
	id: 'umb-property-editor-ui-eye-dropper',
} as Meta;

export const AAAOverview: StoryFn<UmbPropertyEditorUIEyeDropperElement> = () =>
	html`<umb-property-editor-ui-eye-dropper></umb-property-editor-ui-eye-dropper>`;
AAAOverview.storyName = 'Overview';
