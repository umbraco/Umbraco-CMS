import './donut-slice.element';
import './donut-chart.element';

import { Meta } from '@storybook/web-components';
import { html } from 'lit';

export default {
	title: 'Components/Donut chart',
	component: 'umb-donut-chart',
	id: 'umb-donut-chart',
	tags: ['autodocs'],
} as Meta;

export const AAAOverview = () => html` <umb-donut-chart description="Colors of fruits">
	<umb-donut-slice color="red" name="Red" amount="10" kind="apples"></umb-donut-slice>
	<umb-donut-slice color="green" name="Green" amount="20" kind="apples"></umb-donut-slice>
	<umb-donut-slice color="yellow" name="Yellow" amount="10" kind="bananas"></umb-donut-slice>
	<umb-donut-slice color="purple" name="Purple" amount="69" kind="plums"></umb-donut-slice>
</umb-donut-chart>`;
AAAOverview.storyName = 'Overview';
