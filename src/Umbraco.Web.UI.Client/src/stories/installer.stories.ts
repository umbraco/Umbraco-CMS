import { Story, Meta } from '@storybook/web-components';
import { html } from 'lit-html';
import '../installer/installer.element';

export default {
	title: 'Installer/Installer',
	component: 'umb-installer',
	id: 'installer',
} as Meta;

const Template: Story = () => html`<umb-installer></umb-installer>`;

export const Overview = Template.bind({});
