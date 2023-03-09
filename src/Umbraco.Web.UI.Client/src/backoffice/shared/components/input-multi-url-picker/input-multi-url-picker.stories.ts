import { Meta, StoryObj } from '@storybook/web-components';
import './input-multi-url-picker.element';
import type { UmbInputMultiUrlPickerElement } from './input-multi-url-picker.element';

const meta: Meta<UmbInputMultiUrlPickerElement> = {
    title: 'Components/Inputs/Multi URL Picker',
    component: 'umb-input-multi-url-picker'
};
  
export default meta;
type Story = StoryObj<UmbInputMultiUrlPickerElement>;
  
export const Overview: Story = {
    args: {
        
    }
};
