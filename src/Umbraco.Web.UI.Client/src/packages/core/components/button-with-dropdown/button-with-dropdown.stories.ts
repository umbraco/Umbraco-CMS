import { Meta, Story } from '@storybook/web-components';
import { UmbButtonWithDropdownElement } from './button-with-dropdown.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

export default {
	title: 'Components/Button with dropdown',
	component: 'umb-button-with-dropdown',
	id: 'umb-button-with-dropdown',
} as Meta;

export const AAAOverview: Story<UmbButtonWithDropdownElement> = () => html` <umb-button-with-dropdown>
	Open me
	<div slot="dropdown" style="background: pink;  height: 300px">I am a dropdown</div>
</umb-button-with-dropdown>`;
AAAOverview.storyName = 'Overview';
