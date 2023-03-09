import { Meta, StoryObj } from '@storybook/web-components';
import { html } from 'lit-html';

import './body-layout.element';
import type { UmbBodyLayout } from './body-layout.element';

const meta: Meta<UmbBodyLayout> = {
    title: 'Components/Workspace Layout',
    component: 'umb-body-layout'
};
  
export default meta;
type Story = StoryObj<UmbBodyLayout>;

export const Overview: Story = {	
    render: () =>  html`
	<umb-body-layout>
		<div slot="header"><uui-button color="" look="placeholder">Header slot</uui-button></div>
		<uui-button color="" look="placeholder">Main slot</uui-button>
		<div slot="footer"><uui-button color="" look="placeholder">Footer slot</uui-button></div>
	</umb-body-layout>`
};
