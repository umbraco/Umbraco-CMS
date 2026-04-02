import type { UmbPropertyEditorUIImageCropperElement } from './property-editor-ui-image-cropper.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './property-editor-ui-image-cropper.element.js';

export default {
	title: 'Extension Type/Property Editor UI/Image Cropper',
	component: 'umb-property-editor-ui-image-cropper',
	id: 'umb-property-editor-ui-image-cropper',
} as Meta;

export const Docs: StoryFn<UmbPropertyEditorUIImageCropperElement> = () =>
	html`<umb-property-editor-ui-image-cropper></umb-property-editor-ui-image-cropper>`;
