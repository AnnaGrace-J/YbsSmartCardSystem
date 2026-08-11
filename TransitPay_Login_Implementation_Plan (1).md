# Implementation Plan: TransitPay Authentication & Login/Sign-Up System

## Overview
This document serves as the official implementation guide for building the **TransitPay** authentication interface and integration flow. 

The application **MUST** adopt the exact UI/UX design, layout, color tokens, typography scales, Material Symbols, and styling provided in the HTML prototype below. **Tailwind CSS** with the specified custom theme configuration **MUST** be used.

---

## 1. Design & UI Specification

### 1.1 Requirements
* **Framework / Styling:** Tailwind CSS with `@tailwindcss/forms` plugin.
* **Iconography:** Google Material Symbols Outlined (`commute`, `person`, `phone`, `lock`, `lock_reset`, `arrow_forward`).
* **Typography:** `Inter` font family across all components.
* **Layout Structure:** Card-based authentication layout centered vertically and horizontally on a styled background container (`bg-surface-container`).
* **Interactive Elements:**
  * Input fields with leading icons.
  * Phone number input paired with country code dropdown selector (`+1`, `+44`, `+91`, `+61`).
  * Primary action button with active click scale effects (`active:scale-95`).
  * Subtle borders and custom Material 3 inspired color tokens.

---

## 2. Tailwind CSS Configuration

Developers must include or extend the following theme values in `tailwind.config.js` to ensure visual fidelity:

```javascript
module.exports = {
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        "on-secondary-container": "#0c6e68",
        "inverse-on-surface": "#edf0ff",
        "outline": "#737685",
        "primary-fixed": "#dae2ff",
        "surface-container": "#e8edff",
        "inverse-primary": "#b2c5ff",
        "primary-fixed-dim": "#b2c5ff",
        "primary": "#003d9a",
        "surface-container-low": "#f1f3ff",
        "on-tertiary-fixed-variant": "#812800",
        "surface-bright": "#f9f9ff",
        "error-container": "#ffdad6",
        "tertiary-fixed-dim": "#ffb59b",
        "background": "#f9f9ff",
        "secondary": "#006a64",
        "primary-container": "#2a56b4",
        "surface-variant": "#d7e2ff",
        "secondary-container": "#9deee6",
        "inverse-surface": "#1d3052",
        "error": "#ba1a1a",
        "on-primary": "#ffffff",
        "tertiary-container": "#a33500",
        "on-tertiary-fixed": "#380d00",
        "surface-container-high": "#e0e8ff",
        "surface-container-highest": "#d7e2ff",
        "on-secondary": "#ffffff",
        "on-error": "#ffffff",
        "tertiary": "#7b2600",
        "secondary-fixed-dim": "#84d5cd",
        "secondary-fixed": "#a0f1e9",
        "surface-container-lowest": "#ffffff",
        "on-surface-variant": "#434654",
        "on-background": "#041b3c",
        "on-error-container": "#93000a",
        "on-primary-fixed": "#001848",
        "on-surface": "#041b3c",
        "on-tertiary-container": "#ffc6b2",
        "on-secondary-fixed-variant": "#00504b",
        "on-primary-container": "#c4d2ff",
        "outline-variant": "#c3c6d6",
        "on-tertiary": "#ffffff",
        "surface": "#f9f9ff",
        "on-primary-fixed-variant": "#08409e",
        "on-secondary-fixed": "#00201e",
        "surface-dim": "#cadaff",
        "tertiary-fixed": "#ffdbcf",
        "surface-tint": "#2f59b7"
      },
      borderRadius: {
        "DEFAULT": "0.25rem",
        "lg": "0.5rem",
        "xl": "0.75rem",
        "full": "9999px"
      },
      spacing: {
        "gutter": "16px",
        "base-unit": "4px",
        "xl": "32px",
        "md": "16px",
        "sm": "8px",
        "container-margin-desktop": "40px",
        "container-margin-mobile": "16px",
        "xs": "4px",
        "lg": "24px"
      },
      fontFamily: {
        "label-sm": ["Inter", "sans-serif"],
        "label-md": ["Inter", "sans-serif"],
        "display-lg": ["Inter", "sans-serif"],
        "headline-md": ["Inter", "sans-serif"],
        "headline-lg": ["Inter", "sans-serif"],
        "body-lg": ["Inter", "sans-serif"],
        "body-md": ["Inter", "sans-serif"]
      },
      fontSize: {
        "label-sm": ["12px", { lineHeight: "16px", letterSpacing: "0.02em", fontWeight: "500" }],
        "label-md": ["14px", { lineHeight: "20px", letterSpacing: "0.01em", fontWeight: "600" }],
        "display-lg": ["48px", { lineHeight: "56px", letterSpacing: "-0.02em", fontWeight: "700" }],
        "headline-md": ["24px", { lineHeight: "32px", fontWeight: "600" }],
        "headline-lg": ["32px", { lineHeight: "40px", letterSpacing: "-0.01em", fontWeight: "600" }],
        "body-lg": ["18px", { lineHeight: "28px", fontWeight: "400" }],
        "body-md": ["16px", { lineHeight: "24px", fontWeight: "400" }]
      }
    }
  }
}
```

---

## 3. Reference Component Implementation (HTML Source)

AI agents and front-end engineers **MUST** implement the page according to the exact HTML markup below:

```html
<!DOCTYPE html>
<html class="light" lang="en">
<head>
<meta charset="utf-8"/>
<meta content="width=device-width, initial-scale=1.0" name="viewport"/>
<title>TransitPay - Sign Up</title>
<!-- Tailwind Setup -->
<script src="https://cdn.tailwindcss.com?plugins=forms,container-queries"></script>
<script id="tailwind-config">
  tailwind.config = {
    darkMode: "class",
    theme: {
      extend: {
        "colors": {
          "on-secondary-container": "#0c6e68",
          "inverse-on-surface": "#edf0ff",
          "outline": "#737685",
          "primary-fixed": "#dae2ff",
          "surface-container": "#e8edff",
          "inverse-primary": "#b2c5ff",
          "primary-fixed-dim": "#b2c5ff",
          "primary": "#003d9a",
          "surface-container-low": "#f1f3ff",
          "on-tertiary-fixed-variant": "#812800",
          "surface-bright": "#f9f9ff",
          "error-container": "#ffdad6",
          "tertiary-fixed-dim": "#ffb59b",
          "background": "#f9f9ff",
          "secondary": "#006a64",
          "primary-container": "#2a56b4",
          "surface-variant": "#d7e2ff",
          "secondary-container": "#9deee6",
          "inverse-surface": "#1d3052",
          "error": "#ba1a1a",
          "on-primary": "#ffffff",
          "tertiary-container": "#a33500",
          "on-tertiary-fixed": "#380d00",
          "surface-container-high": "#e0e8ff",
          "surface-container-highest": "#d7e2ff",
          "on-secondary": "#ffffff",
          "on-error": "#ffffff",
          "tertiary": "#7b2600",
          "secondary-fixed-dim": "#84d5cd",
          "secondary-fixed": "#a0f1e9",
          "surface-container-lowest": "#ffffff",
          "on-surface-variant": "#434654",
          "on-background": "#041b3c",
          "on-error-container": "#93000a",
          "on-primary-fixed": "#001848",
          "on-surface": "#041b3c",
          "on-tertiary-container": "#ffc6b2",
          "on-secondary-fixed-variant": "#00504b",
          "on-primary-container": "#c4d2ff",
          "outline-variant": "#c3c6d6",
          "on-tertiary": "#ffffff",
          "surface": "#f9f9ff",
          "on-primary-fixed-variant": "#08409e",
          "on-secondary-fixed": "#00201e",
          "surface-dim": "#cadaff",
          "tertiary-fixed": "#ffdbcf",
          "surface-tint": "#2f59b7"
        },
        "borderRadius": {
          "DEFAULT": "0.25rem",
          "lg": "0.5rem",
          "xl": "0.75rem",
          "full": "9999px"
        },
        "spacing": {
          "gutter": "16px",
          "base-unit": "4px",
          "xl": "32px",
          "md": "16px",
          "sm": "8px",
          "container-margin-desktop": "40px",
          "container-margin-mobile": "16px",
          "xs": "4px",
          "lg": "24px"
        },
        "fontFamily": {
          "label-sm": ["Inter"],
          "label-md": ["Inter"],
          "display-lg": ["Inter"],
          "headline-md": ["Inter"],
          "headline-lg": ["Inter"],
          "body-lg": ["Inter"],
          "body-md": ["Inter"]
        },
        "fontSize": {
          "label-sm": ["12px", { "lineHeight": "16px", "letterSpacing": "0.02em", "fontWeight": "500" }],
          "label-md": ["14px", { "lineHeight": "20px", "letterSpacing": "0.01em", "fontWeight": "600" }],
          "display-lg": ["48px", { "lineHeight": "56px", "letterSpacing": "-0.02em", "fontWeight": "700" }],
          "headline-md": ["24px", { "lineHeight": "32px", "fontWeight": "600" }],
          "headline-lg": ["32px", { "lineHeight": "40px", "letterSpacing": "-0.01em", "fontWeight": "600" }],
          "body-lg": ["18px", { "lineHeight": "28px", "fontWeight": "400" }],
          "body-md": ["16px", { "lineHeight": "24px", "fontWeight": "400" }]
        }
      }
    }
  }
</script>
<!-- Google Fonts & Material Symbols -->
<link href="https://fonts.googleapis.com" rel="preconnect"/>
<link crossorigin="" href="https://fonts.gstatic.com" rel="preconnect"/>
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&amp;display=swap" rel="stylesheet"/>
<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&amp;display=swap" rel="stylesheet"/>
<style>
  .material-symbols-outlined {
    font-variation-settings: 'FILL' 0, 'wght' 400, 'GRAD' 0, 'opsz' 24;
  }
</style>
</head>
<body class="bg-surface-container min-h-screen flex items-center justify-center p-md font-body-md text-on-surface">
<!-- Main Container -->
<main class="w-full max-w-[480px] mx-auto">
  <!-- Registration Card -->
  <div class="bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant p-lg md:p-xl flex flex-col gap-lg">
    <!-- Header -->
    <div class="text-center flex flex-col gap-sm items-center">
      <div class="w-12 h-12 bg-primary-container/10 rounded-full flex items-center justify-center text-primary mb-2">
        <span class="material-symbols-outlined" style="font-size: 28px; font-variation-settings: 'FILL' 1;">commute</span>
      </div>
      <h1 class="font-headline-lg text-headline-lg text-primary">TransitPay</h1>
      <p class="font-body-md text-body-md text-on-surface-variant">Create your account for frictionless mobility.</p>
    </div>
    <!-- Form -->
    <form action="#" class="flex flex-col gap-md" method="POST" onsubmit="event.preventDefault(); alert('OTP Sent!');">
      <!-- Username -->
      <div class="flex flex-col gap-xs">
        <label class="font-label-md text-label-md text-on-surface" for="username">Username</label>
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <span class="material-symbols-outlined text-on-surface-variant" style="font-size: 20px;">person</span>
          </div>
          <input class="block w-full pl-10 pr-3 py-2 border border-outline-variant rounded-lg bg-surface-bright text-on-surface focus:ring-2 focus:ring-primary focus:border-primary transition-colors placeholder:text-on-surface-variant/50 font-body-md" id="username" name="username" placeholder="johndoe" required="" type="text"/>
        </div>
      </div>
      <!-- Phone Number -->
      <div class="flex flex-col gap-xs">
        <label class="font-label-md text-label-md text-on-surface" for="phone">Phone Number</label>
        <div class="flex gap-2">
          <select class="border border-outline-variant rounded-lg bg-surface-bright text-on-surface focus:ring-2 focus:ring-primary focus:border-primary py-2 pl-3 pr-8 font-body-md w-24" id="country-code" name="country-code">
            <option value="+1">+1</option>
            <option value="+44">+44</option>
            <option value="+91">+91</option>
            <option value="+61">+61</option>
          </select>
          <div class="relative flex-1">
            <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <span class="material-symbols-outlined text-on-surface-variant" style="font-size: 20px;">phone</span>
            </div>
            <input class="block w-full pl-10 pr-3 py-2 border border-outline-variant rounded-lg bg-surface-bright text-on-surface focus:ring-2 focus:ring-primary focus:border-primary transition-colors placeholder:text-on-surface-variant/50 font-body-md" id="phone" name="phone" placeholder="(555) 000-0000" required="" type="tel"/>
          </div>
        </div>
      </div>
      <!-- Password -->
      <div class="flex flex-col gap-xs">
        <label class="font-label-md text-label-md text-on-surface" for="password">Password</label>
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <span class="material-symbols-outlined text-on-surface-variant" style="font-size: 20px;">lock</span>
          </div>
          <input class="block w-full pl-10 pr-3 py-2 border border-outline-variant rounded-lg bg-surface-bright text-on-surface focus:ring-2 focus:ring-primary focus:border-primary transition-colors placeholder:text-on-surface-variant/50 font-body-md" id="password" name="password" placeholder="••••••••" required="" type="password"/>
        </div>
      </div>
      <!-- Confirm Password -->
      <div class="flex flex-col gap-xs">
        <label class="font-label-md text-label-md text-on-surface" for="confirm_password">Confirm Password</label>
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <span class="material-symbols-outlined text-on-surface-variant" style="font-size: 20px;">lock_reset</span>
          </div>
          <input class="block w-full pl-10 pr-3 py-2 border border-outline-variant rounded-lg bg-surface-bright text-on-surface focus:ring-2 focus:ring-primary focus:border-primary transition-colors placeholder:text-on-surface-variant/50 font-body-md" id="confirm_password" name="confirm_password" placeholder="••••••••" required="" type="password"/>
        </div>
      </div>
      <!-- Action -->
      <button class="mt-sm w-full bg-primary hover:bg-primary/90 text-on-primary font-label-md text-label-md py-3 px-4 rounded-lg transition-all duration-200 active:scale-95 shadow-sm flex items-center justify-center gap-2" type="submit">
        Send OTP
        <span class="material-symbols-outlined" style="font-size: 18px;">arrow_forward</span>
      </button>
    </form>
    <!-- Footer Links -->
    <div class="text-center pt-sm border-t border-outline-variant/50">
      <p class="font-body-md text-body-md text-on-surface-variant">
        Already have an account? 
        <a class="text-primary hover:text-primary-container font-label-md transition-colors" href="#">Log In</a>
      </p>
    </div>
  </div>
</main>
</body>
</html>
```

---

## 4. Implementation Step-by-Step

### Phase 1: Environment Setup
1. Configure Tailwind CSS in your project (React, Next.js, Vue, or static HTML).
2. Install `@tailwindcss/forms` plugin.
3. Update `tailwind.config.js` with theme extensions (colors, font sizes, spacing).
4. Load Google Fonts (`Inter`) and Material Symbols (`Material Symbols Outlined`).

### Phase 2: Component & State Integration
1. **Form Validation:**
   * Ensure `password` and `confirm_password` match before triggering OTP API request.
   * Format `phone` input based on country code selector.
2. **OTP Flow:**
   * Replace the dummy `alert('OTP Sent!')` handler with an asynchronous endpoint integration (`/api/v1/auth/send-otp`).
   * Show dynamic loading/spinner states on the submission button upon submit.
3. **Responsive Verification:**
   * Test card padding (`p-lg` on mobile, `p-xl` on medium+ screens).
   * Ensure max container width does not exceed `480px`.
