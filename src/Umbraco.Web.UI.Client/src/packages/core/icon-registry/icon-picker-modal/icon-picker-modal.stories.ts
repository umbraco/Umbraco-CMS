import './icon-picker-modal.element.js';

import type { UmbIconPickerModalElement } from './icon-picker-modal.element.js';
import type { UmbIconPickerModalValue } from './icon-picker-modal.token.js';
import type { Meta, StoryFn } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Extension Type/Modal/Icon Picker',
	component: 'umb-icon-picker-modal',
	id: 'umb-icon-picker-modal',
} as Meta;

const value: UmbIconPickerModalValue = {
	color: undefined,
	icon: undefined,
};

export const Docs: StoryFn<UmbIconPickerModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-icon-picker-modal .value=${value}></umb-icon-picker-modal>
`;
