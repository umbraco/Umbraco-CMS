import type { UmbDiscardChangesModalElement } from './discard-changes-modal.element.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './discard-changes-modal.element.js';

export default {
	title: 'Extension Type/Modal/Discard Changes',
	component: 'umb-discard-changes-modal',
	id: 'umb-discard-changes-modal',
} as Meta;

export const DiscardChanges: StoryFn<UmbDiscardChangesModalElement> = () => html`
	<umb-discard-changes-modal></umb-discard-changes-modal>
`;
