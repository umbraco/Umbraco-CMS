import type { UmbPropertyEditorUIEyeDropperElement } from './property-editor-ui-eye-dropper.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-eye-dropper.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Eye Dropper',
	component: 'umb-property-editor-ui-eye-dropper',
	id: 'umb-property-editor-ui-eye-dropper',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIEyeDropperElement> = () =>
	html`<umb-property-editor-ui-eye-dropper></umb-property-editor-ui-eye-dropper>`;
