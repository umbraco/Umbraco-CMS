import type { UmbMbcsSiteCode } from './catalog.js';

export interface UmbMbcsPageCopy {
	headline: string;
	paragraphs: Array<string>;
	aside: string;
}

interface UmbMbcsSiteVoice {
	shop: string;
	customer: string;
}

const VOICES: Record<UmbMbcsSiteCode, UmbMbcsSiteVoice> = {
	LO: { shop: 'Little Ones', customer: 'parents' },
	GU: { shop: 'The Outdoor Shop', customer: 'customers' },
};

const copyFor = (site: UmbMbcsSiteCode): Record<string, UmbMbcsPageCopy> => {
	const { shop, customer } = VOICES[site];

	return {
		'New Customer': {
			headline: `Welcome to ${shop}: Where to Start`,
			paragraphs: [
				`New to ${shop}? This is the quickest way to get to know us. We make clothes that are comfortable, practical and built to be worn again and again, and we try to make choosing them as simple as possible.`,
				`Start with the size guide so the first order fits, then browse the latest arrivals. Every product page lists materials, colours and available sizes, so you can see exactly what you are getting.`,
				`If you would like a hand, our customer service team is happy to help ${customer} find the right thing, whether that is a first basic or a full new wardrobe.`,
			],
			aside: `New arrivals land every week. Come back often or sign up to hear about them first.`,
		},
		Sale: {
			headline: 'Seasonal Favourites, Lower Prices',
			paragraphs: [
				`Our sale is where last season's favourites find a new home. The clothes are the same quality as everything else in the shop, they simply make room for what comes next.`,
				`Stock is limited and popular sizes tend to go first. If something catches your eye, it is worth checking the size availability on the product page before it is gone.`,
				`Sale items follow the same care instructions and return options as everything else, so you can shop with the same confidence as always.`,
			],
			aside: `Looking for something specific? Use the filters on the product pages to narrow down by size and colour.`,
		},
		Guides: {
			headline: 'Practical Advice for Every Occasion',
			paragraphs: [
				site === 'LO'
					? `Growing children need clothes that keep up. Our guides cover the everyday questions: what size to buy, what to give as a gift, and how to dress for the season.`
					: `Good clothes are easier to choose when you know what to look for. Our guides help you find the right size and fit, so what you order is what you keep.`,
				`Each guide is written by the people who design and test our clothes, and they are updated as our collections change.`,
				`Start with the size guide if you are unsure about a fit, and come back to the others whenever you need inspiration.`,
			],
			aside: `Can't find the answer you need? Customer service is only a message away.`,
		},
		'Customer Service': {
			headline: 'Self-Service & Solutions',
			paragraphs: [
				`Most questions about an order can be answered in a minute. Here you will find how shipping works, how to return or exchange something, and which payment options are available.`,
				`Start with the topic that matches your question. The frequently asked questions cover everything from tracking a parcel to caring for your clothes, and the answers are kept up to date.`,
				`If you cannot find what you are looking for, the contact page tells you how to reach our team by phone, email or in one of our stores.`,
			],
			aside: `Opening hours and contact details are on the contact page.`,
		},
		'About Us': {
			headline: 'Designed to Last, Made with Care',
			paragraphs: [
				`${shop} started with a simple idea: clothes should be made well enough to be worn for years, not weeks. We design in Denmark and work with carefully chosen partners who share that view.`,
				`We pay attention to materials, fit and finish, and we are open about where and how our clothes are made. Our sustainability page explains our goals and the progress we have made so far.`,
				`Behind the brand is a team of designers, buyers and store staff who care about the same thing you do: getting dressed should be easy, comfortable and a little bit enjoyable.`,
			],
			aside: `Curious about working with us? Have a look at the open positions under Careers.`,
		},
		'My Account': {
			headline: 'Your Orders, Wishlist & Details',
			paragraphs: [
				`Your account keeps everything in one place. Follow your orders from confirmation to delivery, and look back at what you have bought before.`,
				`Save items to your wishlist to come back to later, and keep your delivery addresses up to date so checkout takes seconds.`,
				`You decide what we know about you. Your details are used only to process your orders and to improve your experience.`,
			],
			aside: `Need to change something on an order? Contact customer service as soon as you can.`,
		},
		Basket: {
			headline: 'Review Your Order Before Checkout',
			paragraphs: [
				`Your basket shows everything you have chosen, with sizes, colours and quantities. Take a moment to check that it all looks right before you continue.`,
				`You can change quantities or remove items at any time. Delivery options and payment methods are shown at checkout, before you confirm the order.`,
				`When you have placed your order, you will get a confirmation by email with everything you need to follow it.`,
			],
			aside: `Items in your basket are not reserved until the order is placed.`,
		},
		Legal: {
			headline: 'Terms, Privacy & Cookies',
			paragraphs: [
				`This section collects the documents that describe how ${shop} works with you. They are written to be read, so we keep the language as plain as the subject allows.`,
				`The terms and conditions describe what you can expect when you shop with us. The privacy policy explains which personal data we collect, why we collect it and what your rights are under the GDPR.`,
				`The cookie policy describes which cookies we use on the website and how you can change your choices at any time.`,
			],
			aside: `Questions about your data? Contact us and we will help.`,
		},
		Lookbook: {
			headline: 'Outfits for the Season Ahead',
			paragraphs: [
				`The lookbook shows how our pieces work together, from the first cold morning to a weekend away. It is meant as inspiration rather than a rulebook.`,
				`Each edit is built around a few core pieces in a limited palette, so they can be mixed and worn in many ways.`,
				`Every look links back to the products, so you can see materials and sizes for anything that catches your eye.`,
			],
			aside: `New looks are added with each season.`,
		},
		'Repair Service': {
			headline: 'Repair, Don’t Replace',
			paragraphs: [
				`A good jacket or a favourite sweater should not end up in a drawer because of a broken zip or a worn elbow. Our repair service gives well-made clothes a longer life.`,
				`Book a repair online, send your garment in or hand it in at one of our stores, and our tailors will take care of the rest. The care instructions explain how to look after your clothes between repairs.`,
				`Our lifetime guarantee describes what is covered and how to use it.`,
			],
			aside: `Not sure if something can be repaired? Ask us before you give up on it.`,
		},
	};
};

export const getPageCopy = (site: UmbMbcsSiteCode, pageName: string): UmbMbcsPageCopy | undefined =>
	copyFor(site)[pageName];
