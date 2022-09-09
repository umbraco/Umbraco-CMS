import { Meta } from '@storybook/web-components';
import { html } from 'lit-html';

import '.';

export default {
	title: 'Components/Installer',
	component: 'umb-installer',
	id: 'umb-installer',
} as Meta;

export const Installer = () => html`<umb-installer></umb-installer>`;
