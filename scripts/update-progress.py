#!/usr/bin/env python3
"""
Update progress.yaml based on GitHub issues and pull requests
"""

import os
import yaml
import requests
from datetime import datetime, timezone
from typing import Dict, List, Any

GITHUB_API = "https://api.github.com"
GITHUB_TOKEN = os.environ.get("GITHUB_TOKEN", "")
REPO = os.environ.get("GITHUB_REPOSITORY", "owner/TopicTracker")

headers = {
    "Authorization": f"token {GITHUB_TOKEN}",
    "Accept": "application/vnd.github.v3+json"
}

def get_issues() -> List[Dict[str, Any]]:
    """Fetch all issues from GitHub"""
    url = f"{GITHUB_API}/repos/{REPO}/issues"
    params = {"state": "all", "per_page": 100}
    response = requests.get(url, headers=headers, params=params)
    return response.json()

def get_project_metrics(issues: List[Dict[str, Any]]) -> Dict[str, Any]:
    """Calculate project metrics from issues"""
    total = len([i for i in issues if not i.get("pull_request")])
    completed = len([i for i in issues if i["state"] == "closed" and not i.get("pull_request")])
    in_progress = len([i for i in issues if i["state"] == "open" and 
                      any(label["name"] == "in-progress" for label in i["labels"])])
    blocked = len([i for i in issues if i["state"] == "open" and 
                   any(label["name"] == "blocked" for label in i["labels"])])
    
    return {
        "total_tasks": total,
        "completed_tasks": completed,
        "in_progress_tasks": in_progress,
        "blocked_tasks": blocked
    }

def calculate_velocity(issues: List[Dict[str, Any]]) -> float:
    """Calculate velocity (tasks completed per day)"""
    closed_issues = [i for i in issues if i["state"] == "closed" and not i.get("pull_request")]
    if not closed_issues:
        return 0.0
    
    # Get issues closed in last 7 days
    seven_days_ago = datetime.now(timezone.utc).timestamp() - (7 * 24 * 60 * 60)
    recent_closed = [i for i in closed_issues 
                     if datetime.fromisoformat(i["closed_at"].replace("Z", "+00:00")).timestamp() > seven_days_ago]
    
    return len(recent_closed) / 7.0

def get_phase_status(issues: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
    """Determine status of each development phase"""
    phases = []
    phase_mapping = {
        1: ["Core Data Models", "Thread-Safe Message Store", "Mock SNS Endpoint", "Result Pattern Integration"],
        2: ["Basic Test Client", "Verification Methods", "TUnit Test Helpers"],
        3: ["Query Endpoints", "Advanced Queries", "ASP.NET Core Integration"],
        4: ["Basic Web UI", "Advanced UI Features", "Developer Tools"],
        5: ["Aspire Resource", "Aspire Dashboard"],
        6: ["Performance Optimization", "NuGet Package", "Documentation"]
    }
    
    phase_names = {
        1: "Core Foundation",
        2: "Testing Infrastructure",
        3: "API & Integration",
        4: "Developer Experience",
        5: "Platform Integration",
        6: "Production Readiness"
    }
    
    for phase_num, task_titles in phase_mapping.items():
        phase_issues = [i for i in issues if any(title in i["title"] for title in task_titles)]
        total = len(phase_issues)
        completed = len([i for i in phase_issues if i["state"] == "closed"])
        
        status = "not_started"
        if completed == total and total > 0:
            status = "completed"
        elif completed > 0:
            status = "in_progress"
        elif any(i["state"] == "open" and any(label["name"] == "in-progress" for label in i["labels"]) 
                for i in phase_issues):
            status = "in_progress"
            
        phases.append({
            "phase": phase_num,
            "name": phase_names[phase_num],
            "status": status,
            "tasks_total": total,
            "tasks_completed": completed
        })
    
    return phases

def update_progress_file():
    """Update the progress.yaml file with current metrics"""
    progress_file = "context/TopicTracker/progress.yaml"
    
    # Load existing progress
    with open(progress_file, 'r') as f:
        progress = yaml.safe_load(f)
    
    # Fetch current data
    issues = get_issues()
    metrics = get_project_metrics(issues)
    velocity = calculate_velocity(issues)
    phases = get_phase_status(issues)
    
    # Update progress
    progress["current_status"]["last_updated"] = datetime.now(timezone.utc).isoformat()
    progress["metrics"].update(metrics)
    progress["metrics"]["velocity"]["current"] = round(velocity, 2)
    progress["phases"] = phases
    
    # Determine current phase
    for i, phase in enumerate(phases):
        if phase["status"] == "in_progress":
            progress["current_status"]["phase"] = phase["phase"]
            progress["current_status"]["phase_name"] = phase["name"]
            break
    
    # Update health status
    if progress["metrics"]["blocked_tasks"] > 2:
        progress["current_status"]["health"] = "red"
    elif progress["metrics"]["blocked_tasks"] > 0:
        progress["current_status"]["health"] = "yellow"
    else:
        progress["current_status"]["health"] = "green"
    
    # Write updated progress
    with open(progress_file, 'w') as f:
        yaml.dump(progress, f, default_flow_style=False, sort_keys=False)
    
    print(f"Progress updated: {metrics['completed_tasks']}/{metrics['total_tasks']} tasks completed")
    print(f"Current velocity: {velocity:.2f} tasks/day")

if __name__ == "__main__":
    update_progress_file()