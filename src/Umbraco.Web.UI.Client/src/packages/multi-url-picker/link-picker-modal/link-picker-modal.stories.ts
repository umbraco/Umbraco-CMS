import '../../core/components/body-layout/body-layout.element.js';
import './link-picker-modal.element.js';

import type { UmbLinkPickerModalElement } from './link-picker-modal.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'API/Modals/Layouts/Link Picker',
	component: 'umb-link-picker-modal',
	id: 'umb-link-picker-modal',
} as Meta;

export const Overview: StoryFn<UmbLinkPickerModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-link-picker-modal></umb-link-picker-modal>
`;
