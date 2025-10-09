import type { UmbErrorViewerModalElement } from './error-viewer-modal.element.js';
import type { UmbErrorViewerModalData } from './error-viewer-modal.token.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './error-viewer-modal.element.js';

export default {
	title: 'Extension Type/Modal/Error Viewer',
	component: 'umb-error-viewer-modal',
	id: 'umb-error-viewer-modal',
} as Meta;

const errorData: UmbErrorViewerModalData = {
	headline: 'Error',
	message: 'An unexpected error occurred while processing your request.',
};

export const ErrorViewer: StoryFn<UmbErrorViewerModalElement> = () => html`
	<umb-error-viewer-modal .data=${errorData}></umb-error-viewer-modal>
`;
