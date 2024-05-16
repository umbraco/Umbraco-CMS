import type { Meta } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Apps/Preview',
	component: 'umb-preview',
	id: 'umb-preview',
} satisfies Meta;

export const Preview = () => html`<umb-preview></umb-preview>`;
